<Query Kind="Program">
  <NuGetReference>Chance.NET</NuGetReference>
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <Namespace>ChanceNET</Namespace>
  <Namespace>ChanceNET.Attributes</Namespace>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>Newtonsoft.Json.Bson</Namespace>
  <Namespace>Newtonsoft.Json.Converters</Namespace>
  <Namespace>Newtonsoft.Json.Linq</Namespace>
  <Namespace>Newtonsoft.Json.Schema</Namespace>
  <Namespace>Newtonsoft.Json.Serialization</Namespace>
</Query>

void Main()
{
	// send random data
	Chance chance = new Chance(42);
	
	var sb = new StringBuilder();

	for (int i = 0; i < 1; i++)
	{
		var payload = chance.Object<SamplePayload>();
		payload.ValueNumeric = chance.Integer(250, 300);
		payload.ValueVarchar = payload.ValueNumeric.ToString();
		payload.Temperature = chance.Integer(75, 100);
		payload.TagKey = "58418";
		payload.TimeStamp = DateTime.UtcNow.AddDays(-1).AddSeconds(i);
		payload.ProcessedTimestamp = payload.TimeStamp.AddHours(chance.Integer(1, 10)).AddMinutes(chance.Integer(1, 59));
		string msg = JsonConvert.SerializeObject(payload);
		Console.WriteLine(msg);
		
		sb.AppendLine(msg);
	}

	File.WriteAllText(@"C:\temp\data.json",sb.ToString());

}

public class SamplePayload
{
	[ChanceNET.Attributes.Date(2020, Month.January, 01, 2019, 2025)]
	public DateTime TimeStamp { get; set; }

	[ChanceNET.Attributes.Date(2020, Month.January, 01, 2019, 2025)]
	public DateTime ProcessedTimestamp { get; set; }

	public string ValueVarchar { get; set; }

	public decimal ValueNumeric { get; set; }


	public Int32 Temperature { get; set; }

	public string TagKey { get; set; }

}