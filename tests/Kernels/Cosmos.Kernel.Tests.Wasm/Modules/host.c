// Calls back into the host once per iteration, to time the VM's host boundary.

__attribute__((import_module("env"), import_name("host_add")))
int host_add(int a, int b);

__attribute__((export_name("host_calls")))
int host_calls(int n)
{
    int acc = 0;
    for (int i = 0; i < n; i++)
    {
        acc = host_add(acc, i);
    }
    return acc;
}
