import {
  CognitoIdentityProviderClient,
  ConfirmSignUpCommand,
  InitiateAuthCommand,
  SignUpCommand,
} from "@aws-sdk/client-cognito-identity-provider";

const client = new CognitoIdentityProviderClient({
  region: "eu-central-1",
  endpoint: "http://localhost:4566",
  credentials: {
    accessKeyId: "test",
    secretAccessKey: "test",
  },
});

export async function loginWithCognito(username: string, password: string) {
  const command = new InitiateAuthCommand({
    AuthFlow: "USER_PASSWORD_AUTH",
    ClientId: "QxlSqnYq30Xd3vwSA1lCgGihtt",
    AuthParameters: {
      USERNAME: username,
      PASSWORD: password,
    },
  });

  const response = await client.send(command);
  return response.AuthenticationResult;
}

export async function confirmSignUp(email: string, code: string) {
  const command = new ConfirmSignUpCommand({
    ClientId: "QxlSqnYq30Xd3vwSA1lCgGihtt",
    Username: email,
    ConfirmationCode: code,
  });

  return await client.send(command);
}

export async function signUp(email: string, password: string) {
  const command = new SignUpCommand({
    ClientId: "QxlSqnYq30Xd3vwSA1lCgGihtt",
    Username: email,
    Password: password,
    UserAttributes: [{ Name: "email", Value: email }],
  });

  const response = await client.send(command);
  return response;
}
