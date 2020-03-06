<Query Kind="Program">
  <Connection>
    <ID>efac97da-4fb5-48f7-8ddf-55410fdf0884</ID>
    <Persist>true</Persist>
    <Driver Assembly="DynamicLinqPadPostgreSqlDriver" PublicKeyToken="b79463f4d947ddea">DynamicLinqPadPostgreSqlDriver.DynamicPostgreSqlDriver</Driver>
    <DisplayName>timescaledb</DisplayName>
    <Server>localhost:8081</Server>
    <UserName>postgres</UserName>
    <Password>AQAAANCMnd8BFdERjHoAwE/Cl+sBAAAAQAxQxhxpGE2DG9lAYV38XAAAAAACAAAAAAAQZgAAAAEAACAAAADOJnEwcDbqKXONCn7Plg8MLlZa+WngnW+SNrieBwetlwAAAAAOgAAAAAIAACAAAADVf9chv9QhJrtjmjBfgGHm4hrfcgtqrlJbOMoTG+fShRAAAAD97OASVmrS67FQCv8IZKUtQAAAAN3J7UPAcZgOGnWIR2j/SgTcEqJDCPpqUXsRD0KoNBq4Uyx/UAADAvn2trNiNGheo9g6tBB5WjZCR16Dh/3piw0=</Password>
    <Database>postgres</Database>
    <DriverData>
      <PluralizeSetAndTableProperties>True</PluralizeSetAndTableProperties>
      <SingularizeEntityNames>True</SingularizeEntityNames>
      <CapitalizePropertiesTablesAndColumns>True</CapitalizePropertiesTablesAndColumns>
      <UseExperimentalTypes>False</UseExperimentalTypes>
    </DriverData>
  </Connection>
  <NuGetReference>Npgsql</NuGetReference>
  <Namespace>Npgsql</Namespace>
  <Namespace>NpgsqlTypes</Namespace>
  <Namespace>System</Namespace>
</Query>


async System.Threading.Tasks.Task Main()
{
	
	//this.ConnectionString.Dump();
	//return;

	try
	{
		//var connString = "Host=localhost:8081;Username=postgres;Password=m5asuFHqBE;Database=postgres";
		
		string connString = "Server=localhost;Port=8081;Database=postgres;User Id=postgres;Password=m5asuFHqBE;";
		
		Payload p = new Payload();
		p.Temperature = 99;
		p.ProcessedTimestamp = DateTime.Now;
		p.TimeStamp = DateTime.Now;
		p.ValueNumeric =44;
		p.ValueVarchar = "ss";
		p.TagKey = "1233";

		using (var conn = new NpgsqlConnection(connString))
		{
			await conn.OpenAsync();
			// Insert some data
			using (var cmd =
				new NpgsqlCommand(
					$"insert into Table_001 VALUES ('{p.TimeStamp}', '{p.ValueVarchar}',{p.ValueNumeric},{p.Temperature},'{p.ProcessedTimestamp}','{p.TagKey}')",
					conn))
			{
				await cmd.ExecuteNonQueryAsync();
			}
		}
	}
	catch (Exception e)
	{
		Console.WriteLine(e);
		 
	}
}

public class Payload
{
	public DateTime TimeStamp { get; set; }

	public DateTime ProcessedTimestamp { get; set; }

	public string ValueVarchar { get; set; }

	public decimal ValueNumeric { get; set; }


	public Int32 Temperature { get; set; }

	public string TagKey { get; set; }

}
