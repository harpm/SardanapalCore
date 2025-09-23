// Licensed under the MIT license.


namespace Sardanapal.Localization;

public class ResourceHelper
{
    public static string CraeteRabbitMQMessageHandled(string id, string date)
    {
        return string.Format(Messages.RabbitMQMessageHandled, id, date);
    }

    public static string CraeteRabbitMQMessagePublished(string id, string date)
    {
        return string.Format(Messages.RabbitMQMessagePublished, id, date);
    }
}
