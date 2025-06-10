
using System.Net.Mail;

namespace Sardanapal.Contract.IService;

public interface IEmailService : ISenderService
{
    void Send(string target, string subject, string body, CancellationToken ct = default);
    void Send(string origin, string target, string subject, string body, CancellationToken ct = default);
}

public abstract class EmailServiceBase : IEmailService
{
    protected abstract string OriginAddress { get; }
    protected abstract string DefaultSubject { get; }
    protected virtual SmtpClient client { get; set; }

    protected EmailServiceBase(SmtpClient _client)
    {
        client = _client;
    }

    public virtual void Send(string target, string body, CancellationToken ct = default)
    {
        client.Send(CreateMessage(target, body));
    }

    public virtual void Send(string target, string subject, string body, CancellationToken ct = default)
    {
        client.Send(CreateMessage(target, subject, body));
    }

    public virtual void Send(string origin, string target, string subject, string body, CancellationToken ct = default)
    {
        client.Send(origin, target, subject, body);
    }

    protected virtual MailMessage CreateMessage(string target, string body, CancellationToken ct = default)
    {
        return new MailMessage(OriginAddress, target, DefaultSubject, body);
    }
    protected virtual MailMessage CreateMessage(string target, string subject, string body, CancellationToken ct = default)
    {
        return new MailMessage(OriginAddress, target, DefaultSubject, body);
    }
}