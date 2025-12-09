namespace MIT.Fwk.Infrastructure.Entities
{
    public class MigrationStringWriter
    {
        public static string _lastMigration = "";

        public MigrationStringWriter()
        {
        }

        public void Write(string message)
        {
            _lastMigration = message;
        }

        public string Read()
        {
            return _lastMigration;
        }
    }
}
