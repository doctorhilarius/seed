@ECHO OFF

openssl genrsa -des3 -out seedRootCA.key 2048

openssl req -x509 -new -nodes -key seedRootCA.key -sha256 -days 1024 -out seedRootCA.pem

openssl req -new -sha256 -nodes -out seed.csr -newkey rsa:2048 -keyout seed.key -config seed.csr.cnf

openssl x509 -req -in seed.csr -CA seedRootCA.pem -CAkey seedRootCA.key -CAcreateserial -out seed.crt -days 500 -sha256 -extfile seed.v3.ext
