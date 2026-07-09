pipeline {
    agent any

    environment {
        // Define environment variables if needed
        ASPNETCORE_ENVIRONMENT = 'Development'
        COMPOSE_PROJECT_NAME = 'easypay'
    }

    stages {
        stage('Checkout') {
            steps {
                // Checkout code from source control
                echo 'Checking out source code...'
                checkout scm
            }
        }

        stage('Backend Unit Tests') {
            steps {
                echo 'Running .NET Backend Tests...'
                dir('EasyPay backend') {
                    // Restore dependencies and run tests
                    // We use the docker container context to run tests if we don't want to install .NET SDK on Jenkins directly
                    // Or if .NET SDK is installed on the Jenkins agent:
                    bat 'dotnet restore EasyPay.sln'
                    bat 'dotnet test EasyPay.sln --no-restore --verbosity normal'
                }
            }
        }

        stage('Build Frontend') {
            steps {
                echo 'Building React Frontend...'
                dir('EasyPay frontend') {
                    // Using standard npm commands (Requires Node.js on Jenkins agent)
                    bat 'npm install'
                    bat 'npm run build'
                }
            }
        }

        stage('Docker Build') {
            steps {
                echo 'Building Docker Images...'
                // Build the services defined in docker-compose.yml
                bat 'docker-compose build'
            }
        }

        stage('Deploy') {
            steps {
                echo 'Deploying Application with Docker Compose...'
                // Stop existing containers, remove orphans, and start fresh
                bat 'docker-compose down --remove-orphans'
                bat 'docker-compose up -d'
            }
        }
    }

    post {
        success {
            echo 'Pipeline completed successfully! The EasyPay application is running.'
        }
        failure {
            echo 'Pipeline failed! Please check the logs.'
        }
        always {
            // Optional cleanup steps can go here
            echo 'Cleaning up workspace...'
            cleanWs()
        }
    }
}
