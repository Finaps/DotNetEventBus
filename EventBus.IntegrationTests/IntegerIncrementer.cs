namespace EventBus.IntegrationTests;

public class IntegerIncrementer
{
  public int TimesIncremented { get; set; }

  public async Task Increment()
  {
    int temp = TimesIncremented;
    await Task.Delay(50);
    TimesIncremented = temp + 1;
  }
}
