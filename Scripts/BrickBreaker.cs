using Plasma;
using System.Text.Json;

namespace Game.Scripts;

[PlasmaScript(Id = "671e330c-f424-53c4-99fe-a612b5b04069")]
public sealed partial class BrickBreaker : ComponentScript, IScriptReloadState
{
  [Expose]
  public bool RunMechanicsChecks { get; set; }
  [Expose]
  public float test {get;set;}

  private BrickBreakerSimulation m_Game = new();
  private bool m_RestoredOnReload;
  private double m_Accumulator = 0;
  private GameObject m_Paddle;
  private GameObject m_Ball;
  private readonly GameObject[] m_Bricks = new GameObject[24];
  private InputComponent m_Input;
  private LocalBlackboardComponent m_Hud;

  public override void OnSimulationStarted()
  {
    if (RunMechanicsChecks)
    {
      var checks = new BrickBreakerChecks();
      if (!checks.Run())
        throw new InvalidOperationException($"Brick Breaker: {checks.Failures} mechanics checks failed.");

      Log.Info($"Brick Breaker: {checks.Checks} mechanics checks passed.");
    }

    if (!m_RestoredOnReload)
    {
      m_Game.Reset();
      m_Accumulator = 0;
    }
    m_RestoredOnReload = false;

    m_Paddle = Owner.FindChildByName("Paddle", false);
    m_Ball = Owner.FindChildByName("Ball", false);
    for (int i = 0; i < 24; ++i)
    {
      m_Bricks[i] = Owner.FindChildByName($"Brick {i + 1}", false);
    }

    if (!Owner.TryGetComponent(out m_Input) || !Owner.TryGetComponent(out m_Hud))
      throw new InvalidOperationException("BrickBreaker requires InputComponent and LocalBlackboardComponent on its owner.");
    UpdatePresentation();
  }

  public override void Update(Time deltaTime)
  {
    if (!m_Input.Native.IsAlive || !m_Hud.Native.IsAlive)
      return;

    // The camera faces down the table, so positive world X is screen-left.
    float input = m_Input.GetCurrentInputState("Left", false) - m_Input.GetCurrentInputState("Right", false);
    if (m_Input.GetCurrentInputState("Restart", true) > 0)
    {
      m_Game.Reset();
      m_Accumulator = 0;
    }

    if (m_Input.GetCurrentInputState("Pause", true) > 0)
    {
      m_Game.Paused = !m_Game.Paused;
      m_Accumulator = 0;
    }

    if (m_Input.GetCurrentInputState("Launch", true) > 0)
      m_Game.Launch();

    // Cap catch-up after a stall, preserving the same 120 Hz rules as C++.
    if (!m_Game.Paused)
    {
      m_Accumulator += Math.Min(deltaTime.Seconds, 0.1);
      while (m_Accumulator >= BrickBreakerSimulation.FixedStep)
      {
        m_Game.Step(input);
        m_Accumulator -= BrickBreakerSimulation.FixedStep;
      }
    }

    UpdatePresentation();
  }

  private void UpdatePresentation()
  {
    if (m_Paddle.IsValid)
      m_Paddle.SetLocalPosition(new Vec3(m_Game.PaddleX, BrickBreakerSimulation.PaddleY, 0.34f));

    if (m_Ball.IsValid)
      m_Ball.SetLocalPosition(new Vec3(m_Game.BallX, m_Game.BallY, 0.38f));

    for (int i = 0; i < 24; ++i)
      if (m_Bricks[i].IsValid)
        m_Bricks[i].SetActive(m_Game.Bricks[i]);

    if (!m_Hud.Native.IsValid)
      return;

    m_Hud.SetEntryValue("Score", m_Game.Score);
    m_Hud.SetEntryValue("Lives", m_Game.Lives);
    m_Hud.SetEntryValue("Bricks", m_Game.RemainingBricks());

    string title = "";
    string subtitle = "";
    if (m_Game.Paused)
    {
      title = "PAUSED";
      subtitle = "P or ESC to resume";
    }
    else if (m_Game.State == RoundState.Ready)
    {
      title = m_Game.Lives == 3 ? "CLEAR THE TABLE" : "TRY ANOTHER BALL";
      subtitle = "Move into position, then press SPACE to launch";
    }
    else if (m_Game.State == RoundState.Won)
    {
      title = "TABLE CLEARED";
      subtitle = "24 bricks. Nicely done. Press R to play again.";
    }
    else if (m_Game.State == RoundState.Lost)
    {
      title = "OUT OF BALLS";
      subtitle = "Press R for a fresh table";
    }

    m_Hud.SetEntryValue("ShowPrompt", title.Length != 0);
    m_Hud.SetEntryValue("PromptTitle", title);
    m_Hud.SetEntryValue("PromptSubtitle", subtitle);
  }

  private sealed record ReloadState(int Version, BrickBreakerSimulation Game, double Accumulator);
  private static readonly JsonSerializerOptions s_ReloadJson = new() { IncludeFields = true };

  public string CaptureReloadState() => JsonSerializer.Serialize(new ReloadState(1, m_Game, m_Accumulator), s_ReloadJson);

  public void RestoreReloadState(string state)
  {
    var restored = JsonSerializer.Deserialize<ReloadState>(state, s_ReloadJson)
      ?? throw new InvalidDataException("BrickBreaker reload state is missing.");
    if (restored.Version != 1 || restored.Game.Bricks.Length != BrickBreakerSimulation.BrickCount)
      throw new InvalidDataException("BrickBreaker reload state is incompatible.");
    m_Game = restored.Game;
    m_Accumulator = restored.Accumulator;
    m_RestoredOnReload = true;
  }
}
