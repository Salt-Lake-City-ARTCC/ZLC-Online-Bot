using ZLCBotCore;

try
{
	new ZLCBot().StartAsync().GetAwaiter().GetResult();
}
catch (Exception ex)
{
	Console.WriteLine(ex.Message);
}