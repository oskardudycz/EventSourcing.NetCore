# blocks until kafka is reachable
kafka-topics --bootstrap-server kafka:29092 --list

echo -e 'Creating kafka topics'
kafka-topics --bootstrap-server kafka:29092 --create --if-not-exists --topic Carts --replication-factor 1 --partitions 1
kafka-topics --bootstrap-server kafka:29092 --create --if-not-exists --topic Orders --replication-factor 1 --partitions 1
kafka-topics --bootstrap-server kafka:29092 --create --if-not-exists --topic Payments --replication-factor 1 --partitions 1
kafka-topics --bootstrap-server kafka:29092 --create --if-not-exists --topic Shipments --replication-factor 1 --partitions 1

echo -e 'Successfully created the following topics:'
kafka-topics --bootstrap-server kafka:29092 --list
