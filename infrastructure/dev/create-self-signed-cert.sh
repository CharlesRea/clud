#!/bin/bash
set -e

mkdir -p "$(dirname "$0")/certs"

openssl req -x509 -nodes -days 3650 -newkey rsa:2048 -sha256 -config "$(dirname "$0")/certs/ssl.conf" -keyout "$(dirname "$0")/certs/tls.key" -out "$(dirname "$0")/certs/tls.crt"