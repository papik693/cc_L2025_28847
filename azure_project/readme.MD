## Install Terraform

wget -O - https://apt.releases.hashicorp.com/gpg | sudo gpg --dearmor -o /usr/share/keyrings/hashicorp-archive-keyring.gpg
echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/hashicorp-archive-keyring.gpg] https://apt.releases.hashicorp.com $(grep -oP '(?<=UBUNTU_CODENAME=).*' /etc/os-release || lsb_release -cs) main" | sudo tee /etc/apt/sources.list.d/hashicorp.list
sudo apt update && sudo apt install terraform


## Documentation of azure in terraform
https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs


## Deployment
Run deploy-function-app script with parameters function name and optional resource group. You need zip package to run the script. 
sudo apt install zip

## Database

CREATE TABLE items (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(255) NOT NULL,
    created_at DATETIME2 NOT NULL
); 
