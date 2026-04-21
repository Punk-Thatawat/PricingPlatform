pipeline {
  agent {
    kubernetes {
      defaultContainer 'git'
      yaml '''
apiVersion: v1
kind: Pod
metadata:
  labels:
    app: pricing-platform-ci
spec:
  serviceAccountName: jenkins
  containers:
    - name: kaniko
      image: gcr.io/kaniko-project/executor:debug
      command:
        - /busybox/cat
      tty: true
      resources:
        requests:
          cpu: "500m"
          memory: "1Gi"
        limits:
          cpu: "1500m"
          memory: "2Gi"
      volumeMounts:
        - name: docker-config
          mountPath: /kaniko/.docker
    - name: git
      image: alpine/git:2.45.2
      command:
        - cat
      tty: true
      resources:
        requests:
          cpu: "100m"
          memory: "128Mi"
        limits:
          cpu: "300m"
          memory: "256Mi"
  volumes:
    - name: docker-config
      secret:
        secretName: dockerhub-secret
        items:
          - key: .dockerconfigjson
            path: config.json
'''
    }
  }

  environment {
    REGISTRY = 'docker.io'
    DOCKERHUB_ORG = 'guhodza551'
    MANIFEST_REPO_URL = 'git@github.com:Punk-Thatawat/pricing-platform-manifests.git'
    MANIFEST_REPO_BRANCH = 'main'
    MANIFEST_REPO_DIR = 'manifest-repo'
    PROD_KUSTOMIZATION = 'k8s/overlays/prod/kustomization.yaml'
    GIT_CREDENTIALS_ID = 'manifest-repo-git'
  }

  options {
    timestamps()
    disableConcurrentBuilds()
  }

  stages {
    stage('Checkout App Repo') {
      steps {
        checkout scm
        sh '''
          git config --global --add safe.directory "${WORKSPACE}"
        '''
        script {
          env.SHORT_SHA = sh(script: 'git rev-parse --short=7 HEAD', returnStdout: true).trim()
          env.IMAGE_TAG = "prod-${env.BUILD_NUMBER}-${env.SHORT_SHA}"
        }
      }
    }

    stage('Build And Push Images') {
      steps {
        container('kaniko') {
          sh '''
            /kaniko/executor \
              --context "${WORKSPACE}" \
              --dockerfile "${WORKSPACE}/src/Services/PricingService/Pricing.API/Dockerfile" \
              --destination "${REGISTRY}/${DOCKERHUB_ORG}/pricingplatform-pricingservice:${IMAGE_TAG}" \
              --cache=true \
              --compressed-caching=false

            /kaniko/executor \
              --context "${WORKSPACE}" \
              --dockerfile "${WORKSPACE}/src/Services/RuleService/Rule.API/Dockerfile" \
              --destination "${REGISTRY}/${DOCKERHUB_ORG}/pricingplatform-ruleservice:${IMAGE_TAG}" \
              --cache=true \
              --compressed-caching=false

            /kaniko/executor \
              --context "${WORKSPACE}" \
              --dockerfile "${WORKSPACE}/src/ApiGateway/ApiGateway/Dockerfile" \
              --destination "${REGISTRY}/${DOCKERHUB_ORG}/pricingplatform-apigateway:${IMAGE_TAG}" \
              --cache=true \
              --compressed-caching=false
          '''
        }
      }
    }

    stage('Checkout Manifest Repo') {
      steps {
        container('git') {
          sshagent(credentials: ["${GIT_CREDENTIALS_ID}"]) {
            sh '''
              rm -rf "${WORKSPACE}/${MANIFEST_REPO_DIR}"
              git clone --branch "${MANIFEST_REPO_BRANCH}" "${MANIFEST_REPO_URL}" "${WORKSPACE}/${MANIFEST_REPO_DIR}"
              git config --global --add safe.directory "${WORKSPACE}/${MANIFEST_REPO_DIR}"
            '''
          }
        }
      }
    }

    stage('Update Prod Manifest') {
      steps {
        sh '''
          chmod +x "${WORKSPACE}/scripts/update-prod-kustomize-tags.sh"
          "${WORKSPACE}/scripts/update-prod-kustomize-tags.sh" \
            "${WORKSPACE}/${MANIFEST_REPO_DIR}/${PROD_KUSTOMIZATION}" \
            "${IMAGE_TAG}" \
            "${IMAGE_TAG}" \
            "${IMAGE_TAG}"
        '''
      }
    }

    stage('Commit And Push Manifest Repo') {
      steps {
        container('git') {
          dir("${MANIFEST_REPO_DIR}") {
            sshagent(credentials: ["${GIT_CREDENTIALS_ID}"]) {
              sh '''
                git config user.name "jenkins"
                git config user.email "jenkins@local"

                if git diff --quiet; then
                  echo "No manifest changes to commit"
                  exit 0
                fi

                git add "${PROD_KUSTOMIZATION}"
                git commit -m "chore(prod): deploy ${IMAGE_TAG}"
                git push origin "${MANIFEST_REPO_BRANCH}"
              '''
            }
          }
        }
      }
    }
  }

  post {
    success {
      echo "Images pushed and manifest repo updated. ArgoCD can now sync ${IMAGE_TAG}."
    }
    failure {
      echo "Pipeline failed before ArgoCD sync. Check image push and manifest repo update stages."
    }
  }
}
