terraform {
  required_providers {
    yandex = {
      source = "yandex-cloud/yandex"
    }
    telegram = {
      source = "yi-jiayu/telegram"
      version = "0.3.1"
    }
  }
  required_version = ">= 0.13"
}

provider "yandex" {
  cloud_id                 = var.cloud_id
  folder_id                = var.folder_id
  service_account_key_file = pathexpand("~/.yc-keys/key.json")
}

provider "telegram" {
  bot_token = var.tg_bot_key
}

resource "yandex_iam_service_account" "sa_bucket" {
  name = "sa-bucket-aslkdg"
}

resource "yandex_resourcemanager_folder_iam_binding" "bucket_viewer" {
  folder_id = var.folder_id
  role      = "storage.viewer"
  members = [
    "serviceAccount:${yandex_iam_service_account.sa_bucket.id}"
  ]
}

resource "yandex_storage_bucket" "bucket" {
  bucket = "bucket-lskdbl"
}

resource "yandex_storage_object" "gpt_context" {
  bucket = yandex_storage_bucket.bucket.id
  key    = "context.txt"
  source = "context.txt"
}

data "archive_file" "source" {
  type        = "zip"
  output_path = "function.zip"
  source_dir  = "Function"
  excludes = [
    "**/bin",
    "**/obj"
  ]
}

resource "yandex_function" "function" {
  name              = "hw-1"
  user_hash         = data.archive_file.source.output_md5
  runtime           = "dotnet8"
  memory            = 128
  entrypoint        = "Function.Handler"
  execution_timeout = "10"
  environment = {
    "FOLDER_ID"    = var.folder_id
    "IAM_TOKEN"    = var.iam_token
    "TG_BOT_TOKEN" = var.tg_bot_key
  }
  service_account_id = yandex_iam_service_account.sa_bucket.id
  mounts {
    name = "gpt-settings"
    mode = "ro"
    object_storage {
      bucket = yandex_storage_bucket.bucket.id
    }
  }
  content {
    zip_filename = data.archive_file.source.output_path
  }
}

resource "yandex_function_iam_binding" "function_invoker" {
  function_id = yandex_function.function.id
  role = "functions.functionInvoker"
  members = [
    "system:allUsers"
  ]
}

resource "telegram_bot_webhook" "webhook" {
  url = "https://functions.yandexcloud.net/${yandex_function.function.id}"
}

variable "cloud_id" {
  type = string
}

variable "folder_id" {
  type = string
}

variable "tg_bot_key" {
  type = string
  sensitive = true
}

variable "iam_token" {
  type = string
  sensitive = true
}
