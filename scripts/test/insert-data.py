import psycopg2
import json


payload = '{"TimeStamp":"2020-01-30T22:53:25.3915418Z","ProcessedTimestamp":"2020-01-31T01:09:25.3915418Z","ValueVarchar":"256","ValueNumeric":256.0,"Temperature":88,"TagKey":"58418"}'

data = json.loads(payload)

print(data["TimeStamp"])

""" insert a new vendor into the vendors table """
sql = """insert into Table_001 VALUES ('{TimeStamp}', '{ValueVarchar}',{ValueNumeric},{Temperature},'{ProcessedTimestamp}','{TagKey}')""".format(TimeStamp=data["TimeStamp"],ValueVarchar=data["ValueVarchar"],ValueNumeric=data["ValueNumeric"],Temperature=data["Temperature"],ProcessedTimestamp=data["ProcessedTimestamp"],TagKey=data["TagKey"])

print(sql)

conn = None
vendor_id = None
try:
    # read database configuration
    #params = config()
    # connect to the PostgreSQL database
    conn = psycopg2.connect(host="localhost",database="postgres", user="postgres", password="LgrQE5gXzm2L",port=8081)
    # create a new cursor
    cur = conn.cursor()
    # execute the INSERT statement
    cur.execute(sql)
    # get the generated id back
    #id = cur.fetchone()[0]
    # commit the changes to the database
    conn.commit()
    # close communication with the database
    cur.close()
except (Exception, psycopg2.DatabaseError) as error:
    print(error)
finally:
    if conn is not None:
        conn.close()