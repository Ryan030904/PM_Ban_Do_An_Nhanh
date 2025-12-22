// inside the code that processes a successful login (e.g., btnLogin_Click after validating credentials)
private void OnLoginSuccess()
{
    GlobalVariables.LoggedInUser = authenticatedUser;
    this.DialogResult = DialogResult.OK;
    this.Close();
}