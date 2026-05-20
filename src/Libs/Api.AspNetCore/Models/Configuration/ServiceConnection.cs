using Microsoft.Extensions.Configuration;

namespace Api.AspNetCore.Models.Configuration
{
    public class ServiceConnection
    {
        public string Url { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public static T Parse<T>(IConfiguration configuration, string sectionPath)
            where T : ServiceConnection, new()
        {
            var url = configuration[$"{sectionPath}:Url"];
            var userId = configuration[$"{sectionPath}:UserId"];
            var userName = configuration[$"{sectionPath}:UserName"];
            var password = configuration[$"{sectionPath}:Password"];
            return new T
            {
                Url = url,
                UserId = userId,
                UserName = userName,
                Password = password
            };
        }
    }
}
