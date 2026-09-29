using Game.Scripts;

var checks = new BrickBreakerChecks();
bool passed = checks.Run();
Console.WriteLine($"{checks.Checks} mechanics checks, {checks.Failures} failures.");
return passed ? 0 : 1;
