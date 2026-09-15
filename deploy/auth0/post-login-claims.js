/**
 * Adds the application profile and authorization claims to Auth0 tokens.
 *
 * @param {Event} event
 * @param {PostLoginAPI} api
 */
exports.onExecutePostLogin = async (event, api) => {
  const namespace = "https://ecommerce.local/claims/";

  if (event.user.email) {
    api.accessToken.setCustomClaim(
      `${namespace}email`,
      event.user.email
    );
  }

  api.accessToken.setCustomClaim(
    `${namespace}email_verified`,
    event.user.email_verified === true
  );

  const name =
    event.user.name ||
    event.user.nickname ||
    event.user.email;

  if (name) {
    api.accessToken.setCustomClaim(
      `${namespace}name`,
      name
    );
  }

  const roles = event.authorization?.roles ?? [];

  api.accessToken.setCustomClaim(
    `${namespace}roles`,
    roles
  );

  api.idToken.setCustomClaim(
    `${namespace}roles`,
    roles
  );
};
