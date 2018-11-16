namespace RSSref.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class DataCollections : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MainCollections",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.MainResources",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    ResourceName = c.String(),
                    URL = c.String(),
                    MainCollection_Id = c.Int(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MainCollections", t => t.MainCollection_Id)
                .Index(t => t.MainCollection_Id);
        }

        public override void Down()
        {
            DropForeignKey("dbo.MainResources", "MainCollection_Id", "dbo.MainCollections");
            DropIndex("dbo.MainResources", new[] { "MainCollection_Id" });
            DropTable("dbo.MainResources");
            DropTable("dbo.MainCollections");
        }
    }
}