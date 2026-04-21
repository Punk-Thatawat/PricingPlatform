#!/usr/bin/env sh
set -eu

if [ "$#" -ne 4 ]; then
  echo "Usage: $0 <kustomization-path> <pricing-api-tag> <rule-api-tag> <api-gateway-tag>" >&2
  exit 1
fi

KUSTOMIZATION_PATH="$1"
PRICING_API_TAG="$2"
RULE_API_TAG="$3"
API_GATEWAY_TAG="$4"

awk \
  -v pricing_api_tag="$PRICING_API_TAG" \
  -v rule_api_tag="$RULE_API_TAG" \
  -v api_gateway_tag="$API_GATEWAY_TAG" \
'
  /- name: guhodza551\/pricingplatform-pricingservice/ { service = "pricing-api" }
  /- name: guhodza551\/pricingplatform-ruleservice/ { service = "rule-api" }
  /- name: guhodza551\/pricingplatform-apigateway/ { service = "api-gateway" }
  /newTag:/ {
    if (service == "pricing-api") {
      sub(/newTag: .*/, "newTag: " pricing_api_tag)
    } else if (service == "rule-api") {
      sub(/newTag: .*/, "newTag: " rule_api_tag)
    } else if (service == "api-gateway") {
      sub(/newTag: .*/, "newTag: " api_gateway_tag)
    }
  }
  { print }
' "$KUSTOMIZATION_PATH" > "${KUSTOMIZATION_PATH}.tmp"

mv "${KUSTOMIZATION_PATH}.tmp" "$KUSTOMIZATION_PATH"
