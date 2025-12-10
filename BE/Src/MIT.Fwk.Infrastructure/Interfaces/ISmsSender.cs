using System.Threading.Tasks;

namespace MIT.Fwk.Infrastructure.Interfaces
{
    public interface ISmsSender
    {
        Task SendSmsAsync(string number, string message);
    }
}
