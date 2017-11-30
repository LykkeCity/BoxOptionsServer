using BoxOptions.Common.Models;
using BoxOptions.Core.Interfaces;
using BoxOptions.Services.Models;

namespace BoxOptions.Services
{
    public static class Extensions
    {
        public static IUserItem ToDto(this UserState src)
        {
            return new UserItem
            {
                UserId = src.UserId,
                Balance = src.Balance.ToString(System.Globalization.CultureInfo.InvariantCulture),
                CurrentState = (int)src.CurrentState,
                LastChange = src.LastChange
            };
        }
    }
}
