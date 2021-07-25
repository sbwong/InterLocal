db_string = "host=comp410-postgresql.postgres.database.azure.com port=5432 dbname=postgres user=comp410dbadmin@comp410-postgresql password=Comp410Team4" 
# table defs
psql $db_string -f schema.sql;
# trigger defs
psql $db_string -f updateTriggers.sql;
#set functions
psql $db_string -f functions.sql;
#set indexes
psql $db_string -f index.sql;
#declare the ANON user
psql $db_string -c "INSERT INTO USERS(username, college, email, country, user_status, first_name, last_name, year) VALUES('ANON', '','', '', 'admin', '','', '');"