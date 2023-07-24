namespace eShop.Migrations
{
    using System.Data.Entity.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            ContextKey = "eShop.Models.ApplicationDbContext";
        }

        protected override void Seed(Models.ApplicationDbContext context)
        {

        }
    }
}
