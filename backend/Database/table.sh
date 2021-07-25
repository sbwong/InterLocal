
while getopts t:l: flag
do
    case "${flag}" in
        t) table=${OPTARG};;
        l) limit=${OPTARG};;
    esac
done

psql "host=comp410-postgresql.postgres.database.azure.com port=5432 dbname=postgres user=comp410dbadmin@comp410-postgresql password=Comp410Team4" -c "SELECT * FROM ${table} LIMIT ${limit};"
