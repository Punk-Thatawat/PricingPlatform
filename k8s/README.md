Kubernetes manifests for local testing.

Structure:
- base/: manifests grouped by service or concern
- overlays/: environment-specific customizations

Folders in base:
- namespace/: namespace manifest
- pricing-api/: deployment and service
- rule-api/: deployment and service
- api-gateway/: deployment and service
- ingress/: ingress manifest

Folders in overlays:
- dev/: development overrides
- prod/: production overrides

Notes:
- Update image names if your registry or local cluster uses different tags.
- The gateway routes to pricing-api and rule-api using in-cluster service names.
- Health probes use /health/live and /health/ready.

Example:
- kubectl apply -k k8s/overlays/dev
- kubectl apply -k k8s/overlays/prod
