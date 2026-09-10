const CLAIM_NAMESPACE = "https://ecommerce.local/claims";

exports.onExecutePostLogin = async (event, api) => {
  const email = event.user.email;
  const emailVerified = event.user.email_verified === true;
  const displayName = event.user.name || event.user.nickname || email;
  const roles = event.authorization?.roles || [];

  if (event.authorization) {
    api.accessToken.setCustomClaim(`${CLAIM_NAMESPACE}/roles`, roles);
  }

  api.accessToken.setCustomClaim(`${CLAIM_NAMESPACE}/email`, email);
  api.accessToken.setCustomClaim(`${CLAIM_NAMESPACE}/email_verified`, emailVerified);
  api.accessToken.setCustomClaim(`${CLAIM_NAMESPACE}/name`, displayName);
};
