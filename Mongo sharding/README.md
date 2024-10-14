Проект уже настроен и запускается.

docker compose up -d

Настройка по инструкции скопирована из урока "Реализация шардирования" раздела "Шардирование в MongoDB", но с измененными портами (проверка = пустая трата времени):

#docker exec -it configSrv mongosh --port 27017

rs.initiate( { _id : "config_server", configsvr: true, members: [ { _id : 0, host : "configSrv:27017" } ] } ); exit();

#docker exec -it shard1 mongosh --port 27020

rs.initiate( { _id : "shard1", members: [ { _id : 0, host : "shard1:27020" }, ] } ); exit();

#docker exec -it shard2 mongosh --port 27021

rs.initiate( { _id : "shard2", members: [ { _id : 1, host : "shard2:27021" } ] } ); exit();

#docker exec -it mongos_router mongosh --port 27020

sh.addShard( "shard1/shard1:27020"); sh.addShard( "shard2/shard2:27021");

sh.enableSharding("somedb"); sh.shardCollection("somedb.helloDoc", { "name" : "hashed" } )

use somedb

for(var i = 0; i < 1000; i++) db.helloDoc.insert({age:i, name:"ly"+i})

db.helloDoc.countDocuments(); exit();

Приложение работает и показывает общее количество документов в базе (1000), а также количество документов в каждом из шардов.
