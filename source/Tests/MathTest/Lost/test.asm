test_func:
	push	rbp
	mov	rbp, rsp

	//rbp, rip
	add	rax, rdx

test_func_footer:
	mov	rsp, rbp
	pop	rbp
	add	rsp, params_size
