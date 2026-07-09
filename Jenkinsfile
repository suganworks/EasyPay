pipeline {
    agent any

    environment {
        ASPNETCORE_ENVIRONMENT = 'Development'
        COMPOSE_PROJECT_NAME = 'easypay'
    }

    stages {
        stage('Checkout') {
            steps {
                
                echo 'Checking out source code...'
                checkout scm
            }
        }

        stage('Backend Build & SonarQube') {
            steps {
                echo 'Running SonarQube Analysis and .NET Tests...'
                dir('EasyPay backend') {
                    bat 'dotnet new tool-manifest --force || exit 0'
                    bat 'dotnet tool install dotnet-sonarscanner'
                    
                    bat 'dotnet sonarscanner begin /k:"Easypay" /d:sonar.host.url="http://localhost:9000" /d:sonar.login="sqa_483a409711987bb2812aed130da1e9cdda433e7c"'
                    
                    bat 'dotnet restore EasyPay.sln'
                    
                    bat 'dotnet build EasyPay.sln --no-restore'
                    
                    bat 'dotnet test EasyPay.sln --no-build --verbosity normal'
                    
                    bat 'dotnet sonarscanner end /d:sonar.login="sqa_483a409711987bb2812aed130da1e9cdda433e7c"'
                }
            }
        }

        stage('Build Frontend') {
            steps {
                echo 'Building React Frontend...'
                dir('EasyPay frontend') {
                    bat 'npm install'
                    bat 'npm run build'
                }
            }
        }

        stage('Docker Build') {
            steps {
                echo 'Building Docker Images...'
                bat 'docker-compose build'
            }
        }

        stage('Deploy') {
            steps {
                echo 'Deploying Application with Docker Compose...'
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
            
            echo 'Cleaning up workspace...'
            cleanWs()
        }
    }
}
