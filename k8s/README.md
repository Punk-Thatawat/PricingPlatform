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
- For GitOps with ArgoCD, prefer updating `overlays/prod/kustomization.yaml` image tags in your manifest repo after each successful image push.

Example:
- kubectl apply -k k8s/overlays/dev
- kubectl apply -k k8s/overlays/prod

Jenkins + ArgoCD flow for prod:
- Build and push `guhodza551/pricingplatform-pricingservice`, `guhodza551/pricingplatform-ruleservice`, and `guhodza551/pricingplatform-apigateway` with a unique tag such as `prod-<build>-<sha>`.
- Update `k8s/overlays/prod/kustomization.yaml` in the manifest repo with the new tags.
- Commit and push the manifest repo so ArgoCD detects the change and syncs it to the cluster.
- See `Jenkinsfile.example` and `scripts/update-prod-kustomize-tags.sh` for a working example pipeline.
