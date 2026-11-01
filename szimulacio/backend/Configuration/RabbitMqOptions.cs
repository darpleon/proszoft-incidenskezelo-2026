namespace backend.Configuration;

public class RabbitMqOptions
{
    public string HostName { get; set; } = "localhost";

    public int Port { get; set; } = 5672;

    public string UserName { get; set; } = "rabbitmq";

    public string Password { get; set; } = string.Empty;

    public string Exchange { get; set; } = "events";

    public string RoutingKey { get; set; } = "incident-events";
}
