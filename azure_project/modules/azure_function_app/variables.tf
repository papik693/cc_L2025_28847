variable "codename" {
  description = "Codename to be used in resource names"
  type        = string
}

variable "location" {
  description = "Azure region"
  type        = string
  default     = "West Europe"
}

variable "resource_group_name" {
  description = "Name of existing resource group"
  type        = string
  default     = null
}

variable "enable_application_insights" {
  description = "Enable or disable Application Insights"
  type        = bool
  default     = true
}
