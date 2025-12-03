namespace Expence.Application.Interface
{
    public interface IUserContextService
    {
        string GetUserIdAsync();
        string GetUserEmailAsync();
    }
}
