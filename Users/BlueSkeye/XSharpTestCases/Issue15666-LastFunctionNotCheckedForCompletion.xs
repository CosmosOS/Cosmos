// The last function in this source file is missing a closing curly
// brace. However the compiler doesn't complain. The NASM compilation
// step will fail because the "return" keyword invokes the missing
// label TEST_Last_Exit.
namespace TEST
function Last {
return