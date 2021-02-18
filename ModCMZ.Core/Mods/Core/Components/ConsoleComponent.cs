using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ModCMZ.Core.Mods;
using ModCMZ.Core.Game;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace ModCMZ.Core.Mods.Core.Components
{
	public class ConsoleComponent : DrawableGameComponent, IConsole
	{
		private const float BACKGROUND_OPACITY = .85f;
		private const int MAX_LINES = 100;
		private const int PADDING = 8;
		private const float LINE_HEIGHT = 1.2f;
		private const string CURSOR = "|";
		private const float CURSOR_OFFSET = -.35f;

		private readonly static Color s_CursorColor = Color.White;

		private readonly IList<string> m_UpdateLines = new List<string>(MAX_LINES);
		private readonly ConcurrentQueue<string> m_PendingLines = new ConcurrentQueue<string>();

		private bool _disposed = false;
		private SpriteBatch m_SpriteBatch;
		private Texture2D m_BackgroundTexture;
		private SpriteFont m_Font;
		private Rectangle m_BackgroundArea;
		private Rectangle m_TextArea;
		private int m_ViewportWidth;
		private int m_ViewportHeight;
		private volatile bool m_IsOutputDirty;
		private volatile bool m_IsWidthDirty;
		private volatile bool m_IsInputDirty;
		private StringBuilder m_Input = new StringBuilder(512);
		private Line[] m_CompiledOutputLines = new Line[0];
		private Line[] m_CompiledInputLines = new Line[0];
		private int m_CursorPosition;
		private bool m_IsVisible;

		public new GameApp Game { get; private set; }

		public CoreMod Core { get; private set; }

		public Dictionary<string, ICommand> Commands { get; } = new Dictionary<string, ICommand>();

		public bool IsVisible
		{
			get => m_IsVisible;

			set
			{
				m_IsVisible = value;

				if (value)
				{
					Core.Keyboard.Intercept();
				}
				else
				{
					Core.Keyboard.Release();
				}
			}
		}

		public ConsoleComponent(GameApp game, CoreMod core)
			: base(game.Game)
		{
			Debug.Assert(game.Game != null, "There must be a valid XNA Game associated with the GameApp parent.");

			Game = game;
			Core = core;

			UpdateOrder = -9500;
			DrawOrder = 10000;

			foreach (var command in App.Current.InstantiateModTypes<ICommand>())
            {
				command.Console = this;
				Commands[command.Name.ToLowerInvariant()] = command;
            }
		}

		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					m_SpriteBatch?.Dispose();
					m_BackgroundTexture?.Dispose();

					foreach (var command in Commands.Values)
                    {
						command?.Dispose();
                    }
				}

				Commands.Clear();
			}

			base.Dispose(disposing);
		}

		public void WriteLine(string format, params object[] args)
		{
			WriteLine(string.Format(format, args));
		}

		public void WriteLine()
        {
			WriteLine("");
        }

		public void WriteLine(string text)
		{
			var lines = text.Split('\n').Select(x => x.TrimEnd('\r')).ToArray();

			lock (m_PendingLines)
			{
				foreach (var line in lines)
                {
					m_PendingLines.Enqueue(line);
                }
			}
		}

		public override void Initialize()
		{
			Core.Keyboard.KeyPress += Keyboard_KeyPress;
			Core.Keyboard.StateChanged += Keyboard_StateChanged;

			base.Initialize();
		}

		protected override void LoadContent()
		{
			m_Font = base.Game.Content.Load<SpriteFont>(@"Fonts\ConsoleFont");

			m_BackgroundTexture = new Texture2D(GraphicsDevice, 1, 1);
			m_BackgroundTexture.SetData<Color>(new[] { new Color(0f, 0f, 0f, BACKGROUND_OPACITY) });

			m_SpriteBatch = new SpriteBatch(GraphicsDevice);

			base.LoadContent();
		}

		private void Keyboard_StateChanged(object sender, KeyboardStateChangedEventArgs e)
		{
			if (e.IsNewKeyPressed(Keys.OemTilde))
			{
				IsVisible = !IsVisible;
			}
			else if (IsVisible && e.IsNewKeyReleased(Keys.Escape))
            {
				IsVisible = false;
            }
			else if (!IsVisible && e.IsNewKeyPressed(Keys.OemQuestion))
            {
				IsVisible = true;

				m_Input.Clear().Append("/");
				m_CursorPosition = 1;
				m_IsInputDirty = true;
			}

			// TODO 2021-02-16 Move this out of StateChanged when we find a way to properly intercept WM_KEYDOWN in KeyboardComponent
			if (IsVisible)
            {
				var key = Keys.None;
				var valid = true;
				var modifiers = default(ConsoleModifiers);

				foreach (var k in e.NewState.GetPressedKeys())
                {
					switch (k)
                    {
						case Keys.LeftControl:
						case Keys.RightControl:
							modifiers |= ConsoleModifiers.Control;
							break;

						case Keys.LeftAlt:
						case Keys.RightAlt:
							modifiers |= ConsoleModifiers.Alt;
							break;

						case Keys.LeftShift:
						case Keys.RightShift:
							modifiers |= ConsoleModifiers.Shift;
							break;

						default:
							if (key != Keys.None)
                            {
								valid = false;
                            }

							key = k;
							break;
                    }
                }

				if (valid)
				{
					switch (key)
                    {
						case Keys.Left:
							if (modifiers == 0)
							{
								m_CursorPosition = Math.Max(0, m_CursorPosition - 1);
								m_IsInputDirty = true;
							}
							break;

						case Keys.Right:
							if (modifiers == 0)
							{
								m_CursorPosition = Math.Min(m_Input.Length, m_CursorPosition + 1);
								m_IsInputDirty = true;
							}
							break;

						case Keys.Up:
							break;

						case Keys.Down:
							break;

						case Keys.Delete:
							if (m_CursorPosition < m_Input.Length)
                            {
								m_Input.Remove(m_CursorPosition, 1);
								m_IsInputDirty = true;
                            }
							break;
                    }
                }
            }
		}

		private void Keyboard_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (!IsVisible)
			{
				return;
			}

			if (m_CursorPosition > m_Input.Length)
			{
				m_CursorPosition = m_Input.Length;
			}
			if (m_CursorPosition < 0)
			{
				m_CursorPosition = 0;
			}

			var c = e.KeyChar;
			switch (c)
			{
				case (char)8: // Backspace
					if (m_CursorPosition <= 0)
					{
						return;
					}

					m_Input.Remove(--m_CursorPosition, 1);
					break;

				case (char)13: // Enter
					OnEnter();
					break;

				case (char)27: // Escape
					return;

				case '`':  // Trigger key on US keyboards
					return;

				default:
					if (c >= ' ')
					{
						m_Input.Insert(m_CursorPosition++, c);
						break;
					}

					App.Current.Debug("KeyPress: {0}", (int)c);
					return;
			}

			m_IsInputDirty = true;
		}

		private void OnEnter()
		{
			var command = m_Input.ToString();
			m_Input.Clear();
			m_IsInputDirty = true;

			OnCommand(command);
		}

		private void OnCommand(string rawCommand)
		{
			rawCommand = rawCommand.TrimStart('/');

			if (string.IsNullOrWhiteSpace(rawCommand))
            {
				IsVisible = false;
				return;
            }

			WriteLine(rawCommand);

			var tokens = rawCommand.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

			if (tokens.Length == 0)
            {
				// Should never happen, but just in case
				return;
            }

			var commandName = tokens[0].ToLowerInvariant();

			if (!Commands.TryGetValue(commandName, out var command))
            {
				WriteLine("Command not found: {0}", commandName);
				WriteLine("Available commands: {0}", string.Join(", ", Commands.Keys));
				return;
            }

			try
			{
				command.Run(new CommandArguments(tokens));
			}
			catch (CommandException ex)
            {
				WriteLine("Error: {0}", ex.Message);
            }
		}

		private void UpdatePendingLines()
		{
			for (string line; m_PendingLines.TryDequeue(out line); m_IsOutputDirty = true)
			{
				lock (m_UpdateLines)
				{
					if (m_UpdateLines.Count >= MAX_LINES)
					{
						m_UpdateLines.RemoveAt(0);
					}

					m_UpdateLines.Add(line);
				}
			}
		}

		private Vector2 GetCursorOffset()
		{
			var offset = m_Font.MeasureString(CURSOR);
			offset.Y = 0f;
			offset.X *= CURSOR_OFFSET;
			return offset;
		}

		private Vector2 MeasureString(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				text = " ";
			}

			return m_Font.MeasureString(text);
		}

		private Vector2 MeasureString(StringBuilder text)
		{
			if (text == null || text.Length < 1)
			{
				return m_Font.MeasureString(" ");
			}

			return m_Font.MeasureString(text);
		}

		private IEnumerable<Line> CompileLine(string text, int? cursorPosition = null)
		{
			var chars = text.ToCharArray();
			var line = new StringBuilder(text.Length);
			var lastSize = default(Vector2);
			Vector2? cursorOffset = null;

			if (chars.Length <= 0)
			{
				if (cursorPosition != null)
				{
					cursorOffset = Vector2.Zero + GetCursorOffset();
				}

				yield return new Line(string.Empty, MeasureString(string.Empty), cursorOffset);
				yield break;
			}

			for (var i = 0; i < chars.Length; i++)
			{
				var size = MeasureString(line.Append(chars[i]));

				if (cursorPosition != null)
				{
					if (i == cursorPosition)
					{
						cursorOffset = new Vector2(lastSize.X, 0) + GetCursorOffset();
					}
					else if (cursorPosition >= chars.Length && i == chars.Length - 1)
					{
						cursorOffset = new Vector2(size.X, 0) + GetCursorOffset();
					}
				}

				if (size.X > m_TextArea.Width)
				{
					if (line.Length > 1)
					{
						yield return new Line(line.Remove(line.Length - 1, 1).ToString(), lastSize, cursorOffset);
					}
					else
					{
						yield return new Line(line.ToString(), size, cursorOffset);
					}

					continue;
				}

				lastSize = size;
			}

			yield return new Line(line.ToString(), lastSize, cursorOffset);
		}

		private void UpdateDimensions()
		{
			var viewportWidth = GraphicsDevice.Viewport.Width;
			var viewportHeight = GraphicsDevice.Viewport.Height;

			if (m_ViewportWidth != viewportWidth || m_ViewportHeight != viewportHeight)
			{
				m_IsWidthDirty = m_ViewportWidth != viewportWidth;

				m_BackgroundArea = new Rectangle(0, 0, viewportWidth, viewportHeight * 2 / 3);
				m_TextArea = new Rectangle(PADDING, 0, m_BackgroundArea.Width - PADDING * 2, m_BackgroundArea.Height - PADDING);

				m_ViewportWidth = viewportWidth;
				m_ViewportHeight = viewportHeight;
			}
		}

		private void UpdateCompiledOutputLines()
		{
			var compiledLines = new List<Line>(MAX_LINES * 2);

			foreach (var line in m_UpdateLines)
			{
				compiledLines.AddRange(CompileLine(line));
			}

			m_CompiledOutputLines = compiledLines.Reverse<Line>().ToArray();
		}

		private void UpdateCompiledInputLines()
		{
			m_CompiledInputLines = CompileLine(m_Input.ToString(), m_CursorPosition).Reverse().ToArray();
		}

		public override void Update(GameTime gameTime)
		{
			UpdatePendingLines();

			if (IsVisible)
			{
				UpdateDimensions();

				var widthDirty = m_IsWidthDirty;
				var outputDirty = m_IsOutputDirty;
				var inputDirty = m_IsInputDirty;
				if (widthDirty || outputDirty || inputDirty)
				{
					m_IsWidthDirty = false;
					m_IsOutputDirty = false;
					m_IsInputDirty = false;
				}

				if (widthDirty || outputDirty)
				{
					UpdateCompiledOutputLines();
				}

				if (widthDirty || inputDirty)
				{
					UpdateCompiledInputLines();
				}
			}

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			if (IsVisible)
			{
				m_SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
				try
				{
					m_SpriteBatch.Draw(m_BackgroundTexture, m_BackgroundArea, Color.White);

					var position = new Vector2(m_TextArea.Left, m_TextArea.Bottom);
					foreach (var line in m_CompiledInputLines.Concat(m_CompiledOutputLines))
					{
						if (position.Y < 0)
						{
							break;
						}

						position.Y -= line.Size.Y * LINE_HEIGHT;

						m_SpriteBatch.DrawString(m_Font, line.Text, position, line.Color);

						if (line.CursorOffset != null)
						{
							m_SpriteBatch.DrawString(m_Font, CURSOR, position + line.CursorOffset.Value, s_CursorColor);
						}
					}
				}
				finally
				{
					m_SpriteBatch.End();
				}
			}

			base.Draw(gameTime);
		}

        public void ClearScrollback()
        {
			m_UpdateLines.Clear();
			m_IsOutputDirty = true;
        }

        private struct Line
		{
			public readonly string Text;
			public readonly Vector2 Size;
			public readonly Vector2? CursorOffset;
			public readonly Color Color;

			public Line(string text, Vector2 size, Vector2? cursorOffset)
			{
				Text = text;
				Size = size;
				CursorOffset = cursorOffset;
				Color = Color.LightGray;
			}

			public override string ToString()
			{
				return string.Format("({0} x {1}) {2}", Size.X, Size.Y, Text);
			}

			public override int GetHashCode()
			{
				// In this specific case, we don't need to override GetHashCode.
				return base.GetHashCode();
			}
		}
	}
}
