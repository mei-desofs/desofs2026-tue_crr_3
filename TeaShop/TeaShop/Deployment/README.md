

To run the application over HTTPS locally, you must generate your own local self-signed certificates before starting the containers.

### Prerequisites
Make sure you have `openssl` installed on your machine .

### Setup
**Generate the self-signed certificate and private key:**
   ```bash
   openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout certs/teashop.key -out certs/teashop.crt -subj "/C=US/ST=State/L=City/O=Development/CN=localhost"
   ```
**Start the application:**
   Once the files are generated, you can start the containers normally:
   ```bash
   docker compose up --build
   ```