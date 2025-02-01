namespace GrandChessTree.Toolkit
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("----The Grand Chess Tree Toolkit----");
            await PositionD10Seeder.SeedD10();
            //PositionsD4Generator.GenerateD4HashFenDictionaryValues("out.cs");
        }
    }
}
