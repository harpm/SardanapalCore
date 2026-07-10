// Licensed under the MIT license.


namespace Sardanapal.Localization;

public class ResourceHelper
{
    public static string CreateRabbitMQMessageHandled(string id, string date)
    {
        return string.Format(Messages.RabbitMQMessageHandled, id, date);
    }

    public static string CreateRabbitMQMessagePublished(string id, string date)
    {
        return string.Format(Messages.RabbitMQMessagePublished, id, date);
    }

    public static string CreateNotFoundByKeyMessage(object key)
    {
        return string.Format(Messages.NotFoundByKey, key);
    }
}
