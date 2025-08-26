# Phase 8: Multi-Cloud Terraform Infrastructure
terraform {
  required_version = ">= 1.0"
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
    google = {
      source  = "hashicorp/google"
      version = "~> 4.0"
    }
  }
}

# Variables
variable "environment" {
  description = "Environment name"
  type        = string
  default     = "production"
}

variable "application_name" {
  description = "Application name"
  type        = string
  default     = "vhouse"
}

variable "regions" {
  description = "Deployment regions per cloud provider"
  type = object({
    aws   = list(string)
    azure = list(string)
    gcp   = list(string)
  })
  default = {
    aws   = ["us-east-1", "us-west-2"]
    azure = ["East US", "West Europe"]
    gcp   = ["us-central1", "europe-west1"]
  }
}

# AWS Provider Configuration
provider "aws" {
  alias  = "primary"
  region = var.regions.aws[0]
}

provider "aws" {
  alias  = "secondary"
  region = var.regions.aws[1]
}

# Azure Provider Configuration
provider "azurerm" {
  features {}
  alias = "primary"
}

# GCP Provider Configuration
provider "google" {
  alias   = "primary"
  region  = var.regions.gcp[0]
  project = "vhouse-production"
}

# AWS Infrastructure
module "aws_infrastructure" {
  source = "./modules/aws"
  
  providers = {
    aws.primary   = aws.primary
    aws.secondary = aws.secondary
  }
  
  environment      = var.environment
  application_name = var.application_name
  regions         = var.regions.aws
}

# Azure Infrastructure
module "azure_infrastructure" {
  source = "./modules/azure"
  
  providers = {
    azurerm = azurerm.primary
  }
  
  environment      = var.environment
  application_name = var.application_name
  regions         = var.regions.azure
}

# GCP Infrastructure
module "gcp_infrastructure" {
  source = "./modules/gcp"
  
  providers = {
    google = google.primary
  }
  
  environment      = var.environment
  application_name = var.application_name
  regions         = var.regions.gcp
}

# Global Load Balancer (AWS Route 53 for DNS)
resource "aws_route53_zone" "main" {
  provider = aws.primary
  name     = "vhouse.com"
  
  tags = {
    Environment = var.environment
    Application = var.application_name
  }
}

resource "aws_route53_record" "multi_cloud" {
  provider = aws.primary
  zone_id  = aws_route53_zone.main.zone_id
  name     = "api.vhouse.com"
  type     = "A"
  
  set_identifier = "multi-cloud-failover"
  
  failover_routing_policy {
    type = "PRIMARY"
  }
  
  alias {
    name                   = module.aws_infrastructure.load_balancer_dns
    zone_id                = module.aws_infrastructure.load_balancer_zone_id
    evaluate_target_health = true
  }
}

resource "aws_route53_record" "multi_cloud_secondary" {
  provider = aws.primary
  zone_id  = aws_route53_zone.main.zone_id
  name     = "api.vhouse.com"
  type     = "A"
  
  set_identifier = "multi-cloud-failover-secondary"
  
  failover_routing_policy {
    type = "SECONDARY"
  }
  
  alias {
    name                   = module.azure_infrastructure.load_balancer_dns
    zone_id                = module.azure_infrastructure.load_balancer_zone_id
    evaluate_target_health = true
  }
}

# Global monitoring and alerting
resource "aws_cloudwatch_dashboard" "global" {
  provider       = aws.primary
  dashboard_name = "${var.application_name}-global-${var.environment}"
  
  dashboard_body = jsonencode({
    widgets = [
      {
        type   = "metric"
        x      = 0
        y      = 0
        width  = 12
        height = 6
        
        properties = {
          metrics = [
            ["AWS/ApplicationELB", "RequestCount", "LoadBalancer", module.aws_infrastructure.load_balancer_name],
            ["Microsoft.Network/applicationGateways", "TotalRequests", "ApplicationGateway", module.azure_infrastructure.load_balancer_name],
          ]
          view    = "timeSeries"
          stacked = false
          region  = var.regions.aws[0]
          title   = "Multi-Cloud Request Volume"
          period  = 300
        }
      }
    ]
  })
}

# Outputs
output "aws_endpoints" {
  description = "AWS application endpoints"
  value       = module.aws_infrastructure.endpoints
}

output "azure_endpoints" {
  description = "Azure application endpoints"
  value       = module.azure_infrastructure.endpoints
}

output "gcp_endpoints" {
  description = "GCP application endpoints"
  value       = module.gcp_infrastructure.endpoints
}

output "global_dns" {
  description = "Global DNS endpoint"
  value       = aws_route53_zone.main.name_servers
}

output "monitoring_dashboard" {
  description = "Global monitoring dashboard URL"
  value       = "https://console.aws.amazon.com/cloudwatch/home?region=${var.regions.aws[0]}#dashboards:name=${var.application_name}-global-${var.environment}"
}