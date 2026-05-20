using System.Reflection;
using Data.Repository;

namespace Lockium.Data.LockiumDb.DatabaseContext
{
    public static class LockiumDbContextExtension
    {
        public static bool AllMigrationsApplied(this LockiumDbContext context)
        {
            return context.AllMigrationsAppliedCore();
        }

        public static void EnsureSeeded(this LockiumDbContext context)
        {
            context.EnsureSeededCore(_ =>
                {
                    var dbAssembly = Assembly.GetExecutingAssembly();
                    context.AddSeedFromJson(context.Roles, dbAssembly, "Role", _ => _.Id, null, null, "Data.LockiumDb");
                    context.SaveChanges();
                });
        }
    }
}
