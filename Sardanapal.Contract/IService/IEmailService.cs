
using System.Net.Mail;

namespace Sardanapal.Contract.IService;

public interface IEmailService : ISenderService
{
    void Send(string target, string subject, string body);
    void Send(string origin, string target, string subject, string body);
}

public abstract class EmailServiceBase : IEmailService
{
    protected abstract string OriginAddress { get; }
    protected abstract string DefaultSubject { get; }
    protected SmtpClient client;

    protected EmailServiceBase(SmtpClient _client)
    {
        this.client = _client;
    }

    public void Send(string target, string body)
    {
        client.Send(CreateMessage(target, body));
    }

    public void Send(string target, string subject, string body)
    {
        client.Send(CreateMessage(target, subject, body));
    }

    public void Send(string origin, string target, string subject, string body)
    {
        client.Send(origin, target, subject, body);
    }

    protected MailMessage CreateMessage(string target, string body)
    {
        return new MailMessage(OriginAddress, target, DefaultSubject, body);
    }
    protected MailMessage CreateMessage(string target, string subject, string body)
    {
        return new MailMessage(OriginAddress, target, DefaultSubject, body);
    }
}