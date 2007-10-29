
; OpenGL programming example

format PE GUI 4.0
entry start

include 'win32a.inc'

include 'opengl.inc'

section '.data' data readable writeable

  _title db 'OpenGL example',0
  _class db 'FASMOPENGL32',0

  theta GLfloat 0.6

  wc WNDCLASS 0,WindowProc,0,0,NULL,NULL,NULL,NULL,NULL,_class

  hwnd dd ?
  hdc dd ?
  hrc dd ?

  msg MSG
  rc RECT
  pfd PIXELFORMATDESCRIPTOR

  active dd ?

section '.code' code readable executable

  start:

	invoke	GetModuleHandle,0
	mov	[wc.hInstance],eax
	invoke	LoadIcon,0,IDI_APPLICATION
	mov	[wc.hIcon],eax
	invoke	LoadCursor,0,IDC_ARROW
	mov	[wc.hCursor],eax
	invoke	RegisterClass,wc
	invoke	CreateWindowEx,0,_class,_title,WS_VISIBLE+WS_OVERLAPPEDWINDOW+WS_CLIPCHILDREN+WS_CLIPSIBLINGS,16,16,432,432,NULL,NULL,[wc.hInstance],NULL
	mov	[hwnd],eax

  msg_loop:
	invoke	InvalidateRect,[hwnd],NULL,FALSE
	invoke	GetMessage,msg,NULL,0,0
	or	eax,eax
	jz	end_loop
	invoke	TranslateMessage,msg
	invoke	DispatchMessage,msg
	jmp	msg_loop

  end_loop:
	invoke	ExitProcess,[msg.wParam]

proc WindowProc hwnd,wmsg,wparam,lparam
	push	ebx esi edi
	cmp	[wmsg],WM_CREATE
	je	.wmcreate
	cmp	[wmsg],WM_SIZE
	je	.wmsize
	cmp	[wmsg],WM_ACTIVATEAPP
	je	.wmactivateapp
	cmp	[wmsg],WM_PAINT
	je	.wmpaint
	cmp	[wmsg],WM_KEYDOWN
	je	.wmkeydown
	cmp	[wmsg],WM_DESTROY
	je	.wmdestroy
  .defwndproc:
	invoke	DefWindowProc,[hwnd],[wmsg],[wparam],[lparam]
	jmp	.finish
  .wmcreate:
	invoke	GetDC,[hwnd]
	mov	[hdc],eax
	mov	edi,pfd
	mov	ecx,sizeof.PIXELFORMATDESCRIPTOR shr 2
	xor	eax,eax
	rep	stosd
	mov	[pfd.nSize],sizeof.PIXELFORMATDESCRIPTOR
	mov	[pfd.nVersion],1
	mov	[pfd.dwFlags],PFD_SUPPORT_OPENGL+PFD_DOUBLEBUFFER+PFD_DRAW_TO_WINDOW
	mov	[pfd.dwLayerMask],PFD_MAIN_PLANE
	mov	[pfd.iPixelType],PFD_TYPE_RGBA
	mov	[pfd.cColorBits],16
	mov	[pfd.cDepthBits],16
	mov	[pfd.cAccumBits],0
	mov	[pfd.cStencilBits],0
	invoke	ChoosePixelFormat,[hdc],pfd
	invoke	SetPixelFormat,[hdc],eax,pfd
	invoke	wglCreateContext,[hdc]
	mov	[hrc],eax
	invoke	wglMakeCurrent,[hdc],[hrc]
	invoke	GetClientRect,[hwnd],rc
	invoke	glViewport,0,0,[rc.right],[rc.bottom]
	xor	eax,eax
	jmp	.finish
  .wmsize:
	invoke	GetClientRect,[hwnd],rc
	invoke	glViewport,0,0,[rc.right],[rc.bottom]
	invoke	InvalidateRect,[hwnd],NULL,FALSE
	xor	eax,eax
	jmp	.finish
  .wmactivateapp:
	push	[wmsg]
	pop	[active]
	xor	eax,eax
	jmp	.finish
  .wmpaint:
	invoke	glClear,GL_COLOR_BUFFER_BIT
	invoke	glRotatef,[theta],0.0,0.0,1.0
	invoke	glBegin,GL_QUADS
	invoke	glColor3f,1.0,0.1,0.1
	invoke	glVertex3f,-0.6,-0.6,0.0
	invoke	glColor3f,0.1,0.1,0.1
	invoke	glVertex3f,0.6,-0.6,0.0
	invoke	glColor3f,0.1,0.1,1.0
	invoke	glVertex3f,0.6,0.6,0.0
	invoke	glColor3f,1.0,0.1,1.0
	invoke	glVertex3f,-0.6,0.6,0.0
	invoke	glEnd
	invoke	SwapBuffers,[hdc]
	xor	eax,eax
	jmp	.finish
  .wmkeydown:
	cmp	[wparam],VK_ESCAPE
	jne	.defwndproc
  .wmdestroy:
	invoke	wglMakeCurrent,0,0
	invoke	wglDeleteContext,[hrc]
	invoke	ReleaseDC,[hwnd],[hdc]
	invoke	PostQuitMessage,0
	xor	eax,eax
  .finish:
	pop	edi esi ebx
	ret
endp

section '.idata' import data readable writeable

  library kernel,'KERNEL32.DLL',\
	  user,'USER32.DLL',\
	  gdi,'GDI32.DLL',\
	  opengl,'OPENGL32.DLL',\
	  glu,'GLU32.DLL'

  import kernel,\
	 GetModuleHandle,'GetModuleHandleA',\
	 ExitProcess,'ExitProcess'

  import user,\
	 RegisterClass,'RegisterClassA',\
	 CreateWindowEx,'CreateWindowExA',\
	 DefWindowProc,'DefWindowProcA',\
	 GetMessage,'GetMessageA',\
	 TranslateMessage,'TranslateMessage',\
	 DispatchMessage,'DispatchMessageA',\
	 LoadCursor,'LoadCursorA',\
	 LoadIcon,'LoadIconA',\
	 GetClientRect,'GetClientRect',\
	 InvalidateRect,'InvalidateRect',\
	 GetDC,'GetDC',\
	 ReleaseDC,'ReleaseDC',\
	 PostQuitMessage,'PostQuitMessage'

  import gdi,\
	 ChoosePixelFormat,'ChoosePixelFormat',\
	 SetPixelFormat,'SetPixelFormat',\
	 SwapBuffers,'SwapBuffers'

  import opengl,\
	 glAccum,'glAccum',\
	 glAlphaFunc,'glAlphaFunc',\
	 glAreTexturesResident,'glAreTexturesResident',\
	 glArrayElement,'glArrayElement',\
	 glBegin,'glBegin',\
	 glBindTexture,'glBindTexture',\
	 glBitmap,'glBitmap',\
	 glBlendFunc,'glBlendFunc',\
	 glCallList,'glCallList',\
	 glCallLists,'glCallLists',\
	 glClear,'glClear',\
	 glClearAccum,'glClearAccum',\
	 glClearColor,'glClearColor',\
	 glClearDepth,'glClearDepth',\
	 glClearIndex,'glClearIndex',\
	 glClearStencil,'glClearStencil',\
	 glClipPlane,'glClipPlane',\
	 glColor3b,'glColor3b',\
	 glColor3bv,'glColor3bv',\
	 glColor3d,'glColor3d',\
	 glColor3dv,'glColor3dv',\
	 glColor3f,'glColor3f',\
	 glColor3fv,'glColor3fv',\
	 glColor3i,'glColor3i',\
	 glColor3iv,'glColor3iv',\
	 glColor3s,'glColor3s',\
	 glColor3sv,'glColor3sv',\
	 glColor3ub,'glColor3ub',\
	 glColor3ubv,'glColor3ubv',\
	 glColor3ui,'glColor3ui',\
	 glColor3uiv,'glColor3uiv',\
	 glColor3us,'glColor3us',\
	 glColor3usv,'glColor3usv',\
	 glColor4b,'glColor4b',\
	 glColor4bv,'glColor4bv',\
	 glColor4d,'glColor4d',\
	 glColor4dv,'glColor4dv',\
	 glColor4f,'glColor4f',\
	 glColor4fv,'glColor4fv',\
	 glColor4i,'glColor4i',\
	 glColor4iv,'glColor4iv',\
	 glColor4s,'glColor4s',\
	 glColor4sv,'glColor4sv',\
	 glColor4ub,'glColor4ub',\
	 glColor4ubv,'glColor4ubv',\
	 glColor4ui,'glColor4ui',\
	 glColor4uiv,'glColor4uiv',\
	 glColor4us,'glColor4us',\
	 glColor4usv,'glColor4usv',\
	 glColorMask,'glColorMask',\
	 glColorMaterial,'glColorMaterial',\
	 glColorPointer,'glColorPointer',\
	 glCopyPixels,'glCopyPixels',\
	 glCopyTexImage1D,'glCopyTexImage1D',\
	 glCopyTexImage2D,'glCopyTexImage2D',\
	 glCopyTexSubImage1D,'glCopyTexSubImage1D',\
	 glCopyTexSubImage2D,'glCopyTexSubImage2D',\
	 glCullFace,'glCullFace',\
	 glDeleteLists,'glDeleteLists',\
	 glDeleteTextures,'glDeleteTextures',\
	 glDepthFunc,'glDepthFunc',\
	 glDepthMask,'glDepthMask',\
	 glDepthRange,'glDepthRange',\
	 glDisable,'glDisable',\
	 glDisableClientState,'glDisableClientState',\
	 glDrawArrays,'glDrawArrays',\
	 glDrawBuffer,'glDrawBuffer',\
	 glDrawElements,'glDrawElements',\
	 glDrawPixels,'glDrawPixels',\
	 glEdgeFlag,'glEdgeFlag',\
	 glEdgeFlagPointer,'glEdgeFlagPointer',\
	 glEdgeFlagv,'glEdgeFlagv',\
	 glEnable,'glEnable',\
	 glEnableClientState,'glEnableClientState',\
	 glEnd,'glEnd',\
	 glEndList,'glEndList',\
	 glEvalCoord1d,'glEvalCoord1d',\
	 glEvalCoord1dv,'glEvalCoord1dv',\
	 glEvalCoord1f,'glEvalCoord1f',\
	 glEvalCoord1fv,'glEvalCoord1fv',\
	 glEvalCoord2d,'glEvalCoord2d',\
	 glEvalCoord2dv,'glEvalCoord2dv',\
	 glEvalCoord2f,'glEvalCoord2f',\
	 glEvalCoord2fv,'glEvalCoord2fv',\
	 glEvalMesh1,'glEvalMesh1',\
	 glEvalMesh2,'glEvalMesh2',\
	 glEvalPoint1,'glEvalPoint1',\
	 glEvalPoint2,'glEvalPoint2',\
	 glFeedbackBuffer,'glFeedbackBuffer',\
	 glFinish,'glFinish',\
	 glFlush,'glFlush',\
	 glFogf,'glFogf',\
	 glFogfv,'glFogfv',\
	 glFogi,'glFogi',\
	 glFogiv,'glFogiv',\
	 glFrontFace,'glFrontFace',\
	 glFrustum,'glFrustum',\
	 glGenLists,'glGenLists',\
	 glGenTextures,'glGenTextures',\
	 glGetBooleanv,'glGetBooleanv',\
	 glGetClipPlane,'glGetClipPlane',\
	 glGetDoublev,'glGetDoublev',\
	 glGetError,'glGetError',\
	 glGetFloatv,'glGetFloatv',\
	 glGetIntegerv,'glGetIntegerv',\
	 glGetLightfv,'glGetLightfv',\
	 glGetLightiv,'glGetLightiv',\
	 glGetMapdv,'glGetMapdv',\
	 glGetMapfv,'glGetMapfv',\
	 glGetMapiv,'glGetMapiv',\
	 glGetMaterialfv,'glGetMaterialfv',\
	 glGetMaterialiv,'glGetMaterialiv',\
	 glGetPixelMapfv,'glGetPixelMapfv',\
	 glGetPixelMapuiv,'glGetPixelMapuiv',\
	 glGetPixelMapusv,'glGetPixelMapusv',\
	 glGetPointerv,'glGetPointerv',\
	 glGetPolygonStipple,'glGetPolygonStipple',\
	 glGetString,'glGetString',\
	 glGetTexEnvfv,'glGetTexEnvfv',\
	 glGetTexEnviv,'glGetTexEnviv',\
	 glGetTexGendv,'glGetTexGendv',\
	 glGetTexGenfv,'glGetTexGenfv',\
	 glGetTexGeniv,'glGetTexGeniv',\
	 glGetTexImage,'glGetTexImage',\
	 glGetTexLevelParameterfv,'glGetTexLevelParameterfv',\
	 glGetTexLevelParameteriv,'glGetTexLevelParameteriv',\
	 glGetTexParameterfv,'glGetTexParameterfv',\
	 glGetTexParameteriv,'glGetTexParameteriv',\
	 glHint,'glHint',\
	 glIndexMask,'glIndexMask',\
	 glIndexPointer,'glIndexPointer',\
	 glIndexd,'glIndexd',\
	 glIndexdv,'glIndexdv',\
	 glIndexf,'glIndexf',\
	 glIndexfv,'glIndexfv',\
	 glIndexi,'glIndexi',\
	 glIndexiv,'glIndexiv',\
	 glIndexs,'glIndexs',\
	 glIndexsv,'glIndexsv',\
	 glIndexub,'glIndexub',\
	 glIndexubv,'glIndexubv',\
	 glInitNames,'glInitNames',\
	 glInterleavedArrays,'glInterleavedArrays',\
	 glIsEnabled,'glIsEnabled',\
	 glIsList,'glIsList',\
	 glIsTexture,'glIsTexture',\
	 glLightModelf,'glLightModelf',\
	 glLightModelfv,'glLightModelfv',\
	 glLightModeli,'glLightModeli',\
	 glLightModeliv,'glLightModeliv',\
	 glLightf,'glLightf',\
	 glLightfv,'glLightfv',\
	 glLighti,'glLighti',\
	 glLightiv,'glLightiv',\
	 glLineStipple,'glLineStipple',\
	 glLineWidth,'glLineWidth',\
	 glListBase,'glListBase',\
	 glLoadIdentity,'glLoadIdentity',\
	 glLoadMatrixd,'glLoadMatrixd',\
	 glLoadMatrixf,'glLoadMatrixf',\
	 glLoadName,'glLoadName',\
	 glLogicOp,'glLogicOp',\
	 glMap1d,'glMap1d',\
	 glMap1f,'glMap1f',\
	 glMap2d,'glMap2d',\
	 glMap2f,'glMap2f',\
	 glMapGrid1d,'glMapGrid1d',\
	 glMapGrid1f,'glMapGrid1f',\
	 glMapGrid2d,'glMapGrid2d',\
	 glMapGrid2f,'glMapGrid2f',\
	 glMaterialf,'glMaterialf',\
	 glMaterialfv,'glMaterialfv',\
	 glMateriali,'glMateriali',\
	 glMaterialiv,'glMaterialiv',\
	 glMatrixMode,'glMatrixMode',\
	 glMultMatrixd,'glMultMatrixd',\
	 glMultMatrixf,'glMultMatrixf',\
	 glNewList,'glNewList',\
	 glNormal3b,'glNormal3b',\
	 glNormal3bv,'glNormal3bv',\
	 glNormal3d,'glNormal3d',\
	 glNormal3dv,'glNormal3dv',\
	 glNormal3f,'glNormal3f',\
	 glNormal3fv,'glNormal3fv',\
	 glNormal3i,'glNormal3i',\
	 glNormal3iv,'glNormal3iv',\
	 glNormal3s,'glNormal3s',\
	 glNormal3sv,'glNormal3sv',\
	 glNormalPointer,'glNormalPointer',\
	 glOrtho,'glOrtho',\
	 glPassThrough,'glPassThrough',\
	 glPixelMapfv,'glPixelMapfv',\
	 glPixelMapuiv,'glPixelMapuiv',\
	 glPixelMapusv,'glPixelMapusv',\
	 glPixelStoref,'glPixelStoref',\
	 glPixelStorei,'glPixelStorei',\
	 glPixelTransferf,'glPixelTransferf',\
	 glPixelTransferi,'glPixelTransferi',\
	 glPixelZoom,'glPixelZoom',\
	 glPointSize,'glPointSize',\
	 glPolygonMode,'glPolygonMode',\
	 glPolygonOffset,'glPolygonOffset',\
	 glPolygonStipple,'glPolygonStipple',\
	 glPopAttrib,'glPopAttrib',\
	 glPopClientAttrib,'glPopClientAttrib',\
	 glPopMatrix,'glPopMatrix',\
	 glPopName,'glPopName',\
	 glPrioritizeTextures,'glPrioritizeTextures',\
	 glPushAttrib,'glPushAttrib',\
	 glPushClientAttrib,'glPushClientAttrib',\
	 glPushMatrix,'glPushMatrix',\
	 glPushName,'glPushName',\
	 glRasterPos2d,'glRasterPos2d',\
	 glRasterPos2dv,'glRasterPos2dv',\
	 glRasterPos2f,'glRasterPos2f',\
	 glRasterPos2fv,'glRasterPos2fv',\
	 glRasterPos2i,'glRasterPos2i',\
	 glRasterPos2iv,'glRasterPos2iv',\
	 glRasterPos2s,'glRasterPos2s',\
	 glRasterPos2sv,'glRasterPos2sv',\
	 glRasterPos3d,'glRasterPos3d',\
	 glRasterPos3dv,'glRasterPos3dv',\
	 glRasterPos3f,'glRasterPos3f',\
	 glRasterPos3fv,'glRasterPos3fv',\
	 glRasterPos3i,'glRasterPos3i',\
	 glRasterPos3iv,'glRasterPos3iv',\
	 glRasterPos3s,'glRasterPos3s',\
	 glRasterPos3sv,'glRasterPos3sv',\
	 glRasterPos4d,'glRasterPos4d',\
	 glRasterPos4dv,'glRasterPos4dv',\
	 glRasterPos4f,'glRasterPos4f',\
	 glRasterPos4fv,'glRasterPos4fv',\
	 glRasterPos4i,'glRasterPos4i',\
	 glRasterPos4iv,'glRasterPos4iv',\
	 glRasterPos4s,'glRasterPos4s',\
	 glRasterPos4sv,'glRasterPos4sv',\
	 glReadBuffer,'glReadBuffer',\
	 glReadPixels,'glReadPixels',\
	 glRectd,'glRectd',\
	 glRectdv,'glRectdv',\
	 glRectf,'glRectf',\
	 glRectfv,'glRectfv',\
	 glRecti,'glRecti',\
	 glRectiv,'glRectiv',\
	 glRects,'glRects',\
	 glRectsv,'glRectsv',\
	 glRenderMode,'glRenderMode',\
	 glRotated,'glRotated',\
	 glRotatef,'glRotatef',\
	 glScaled,'glScaled',\
	 glScalef,'glScalef',\
	 glScissor,'glScissor',\
	 glSelectBuffer,'glSelectBuffer',\
	 glShadeModel,'glShadeModel',\
	 glStencilFunc,'glStencilFunc',\
	 glStencilMask,'glStencilMask',\
	 glStencilOp,'glStencilOp',\
	 glTexCoord1d,'glTexCoord1d',\
	 glTexCoord1dv,'glTexCoord1dv',\
	 glTexCoord1f,'glTexCoord1f',\
	 glTexCoord1fv,'glTexCoord1fv',\
	 glTexCoord1i,'glTexCoord1i',\
	 glTexCoord1iv,'glTexCoord1iv',\
	 glTexCoord1s,'glTexCoord1s',\
	 glTexCoord1sv,'glTexCoord1sv',\
	 glTexCoord2d,'glTexCoord2d',\
	 glTexCoord2dv,'glTexCoord2dv',\
	 glTexCoord2f,'glTexCoord2f',\
	 glTexCoord2fv,'glTexCoord2fv',\
	 glTexCoord2i,'glTexCoord2i',\
	 glTexCoord2iv,'glTexCoord2iv',\
	 glTexCoord2s,'glTexCoord2s',\
	 glTexCoord2sv,'glTexCoord2sv',\
	 glTexCoord3d,'glTexCoord3d',\
	 glTexCoord3dv,'glTexCoord3dv',\
	 glTexCoord3f,'glTexCoord3f',\
	 glTexCoord3fv,'glTexCoord3fv',\
	 glTexCoord3i,'glTexCoord3i',\
	 glTexCoord3iv,'glTexCoord3iv',\
	 glTexCoord3s,'glTexCoord3s',\
	 glTexCoord3sv,'glTexCoord3sv',\
	 glTexCoord4d,'glTexCoord4d',\
	 glTexCoord4dv,'glTexCoord4dv',\
	 glTexCoord4f,'glTexCoord4f',\
	 glTexCoord4fv,'glTexCoord4fv',\
	 glTexCoord4i,'glTexCoord4i',\
	 glTexCoord4iv,'glTexCoord4iv',\
	 glTexCoord4s,'glTexCoord4s',\
	 glTexCoord4sv,'glTexCoord4sv',\
	 glTexCoordPointer,'glTexCoordPointer',\
	 glTexEnvf,'glTexEnvf',\
	 glTexEnvfv,'glTexEnvfv',\
	 glTexEnvi,'glTexEnvi',\
	 glTexEnviv,'glTexEnviv',\
	 glTexGend,'glTexGend',\
	 glTexGendv,'glTexGendv',\
	 glTexGenf,'glTexGenf',\
	 glTexGenfv,'glTexGenfv',\
	 glTexGeni,'glTexGeni',\
	 glTexGeniv,'glTexGeniv',\
	 glTexImage1D,'glTexImage1D',\
	 glTexImage2D,'glTexImage2D',\
	 glTexParameterf,'glTexParameterf',\
	 glTexParameterfv,'glTexParameterfv',\
	 glTexParameteri,'glTexParameteri',\
	 glTexParameteriv,'glTexParameteriv',\
	 glTexSubImage1D,'glTexSubImage1D',\
	 glTexSubImage2D,'glTexSubImage2D',\
	 glTranslated,'glTranslated',\
	 glTranslatef,'glTranslatef',\
	 glVertex2d,'glVertex2d',\
	 glVertex2dv,'glVertex2dv',\
	 glVertex2f,'glVertex2f',\
	 glVertex2fv,'glVertex2fv',\
	 glVertex2i,'glVertex2i',\
	 glVertex2iv,'glVertex2iv',\
	 glVertex2s,'glVertex2s',\
	 glVertex2sv,'glVertex2sv',\
	 glVertex3d,'glVertex3d',\
	 glVertex3dv,'glVertex3dv',\
	 glVertex3f,'glVertex3f',\
	 glVertex3fv,'glVertex3fv',\
	 glVertex3i,'glVertex3i',\
	 glVertex3iv,'glVertex3iv',\
	 glVertex3s,'glVertex3s',\
	 glVertex3sv,'glVertex3sv',\
	 glVertex4d,'glVertex4d',\
	 glVertex4dv,'glVertex4dv',\
	 glVertex4f,'glVertex4f',\
	 glVertex4fv,'glVertex4fv',\
	 glVertex4i,'glVertex4i',\
	 glVertex4iv,'glVertex4iv',\
	 glVertex4s,'glVertex4s',\
	 glVertex4sv,'glVertex4sv',\
	 glVertexPointer,'glVertexPointer',\
	 glViewport,'glViewport',\
	 wglGetProcAddress,'wglGetProcAddress',\
	 wglCopyContext,'wglCopyContext',\
	 wglCreateContext,'wglCreateContext',\
	 wglCreateLayerContext,'wglCreateLayerContext',\
	 wglDeleteContext,'wglDeleteContext',\
	 wglDescribeLayerPlane,'wglDescribeLayerPlane',\
	 wglGetCurrentContext,'wglGetCurrentContext',\
	 wglGetCurrentDC,'wglGetCurrentDC',\
	 wglGetLayerPaletteEntries,'wglGetLayerPaletteEntries',\
	 wglMakeCurrent,'wglMakeCurrent',\
	 wglRealizeLayerPalette,'wglRealizeLayerPalette',\
	 wglSetLayerPaletteEntries,'wglSetLayerPaletteEntries',\
	 wglShareLists,'wglShareLists',\
	 wglSwapLayerBuffers,'wglSwapLayerBuffers',\
	 wglSwapMultipleBuffers,'wglSwapMultipleBuffers',\
	 wglUseFontBitmapsA,'wglUseFontBitmapsA',\
	 wglUseFontOutlinesA,'wglUseFontOutlinesA',\
	 wglUseFontBitmapsW,'wglUseFontBitmapsW',\
	 wglUseFontOutlinesW,'wglUseFontOutlinesW',\
	 wglUseFontBitmaps,'wglUseFontBitmaps',\
	 wglUseFontOutlines,'wglUseFontOutlines',\
	 glDrawRangeElements,'glDrawRangeElements',\
	 glTexImage3D,'glTexImage3D',\
	 glBlendColor,'glBlendColor',\
	 glBlendEquation,'glBlendEquation',\
	 glColorSubTable,'glColorSubTable',\
	 glCopyColorSubTable,'glCopyColorSubTable',\
	 glColorTable,'glColorTable',\
	 glCopyColorTable,'glCopyColorTable',\
	 glColorTableParameteriv,'glColorTableParameteriv',\
	 glColorTableParameterfv,'glColorTableParameterfv',\
	 glGetColorTable,'glGetColorTable',\
	 glGetColorTableParameteriv,'glGetColorTableParameteriv',\
	 glGetColorTableParameterfv,'glGetColorTableParameterfv',\
	 glConvolutionFilter1D,'glConvolutionFilter1D',\
	 glConvolutionFilter2D,'glConvolutionFilter2D',\
	 glCopyConvolutionFilter1D,'glCopyConvolutionFilter1D',\
	 glCopyConvolutionFilter2D,'glCopyConvolutionFilter2D',\
	 glGetConvolutionFilter,'glGetConvolutionFilter',\
	 glSeparableFilter2D,'glSeparableFilter2D',\
	 glGetSeparableFilter,'glGetSeparableFilter',\
	 glConvolutionParameteri,'glConvolutionParameteri',\
	 glConvolutionParameteriv,'glConvolutionParameteriv',\
	 glConvolutionParameterf,'glConvolutionParameterf',\
	 glConvolutionParameterfv,'glConvolutionParameterfv',\
	 glGetConvolutionParameteriv,'glGetConvolutionParameteriv',\
	 glGetConvolutionParameterfv,'glGetConvolutionParameterfv',\
	 glHistogram,'glHistogram',\
	 glResetHistogram,'glResetHistogram',\
	 glGetHistogram,'glGetHistogram',\
	 glGetHistogramParameteriv,'glGetHistogramParameteriv',\
	 glGetHistogramParameterfv,'glGetHistogramParameterfv',\
	 glMinmax,'glMinmax',\
	 glResetMinmax,'glResetMinmax',\
	 glGetMinmax,'glGetMinmax',\
	 glGetMinmaxParameteriv,'glGetMinmaxParameteriv',\
	 glGetMinmaxParameterfv,'glGetMinmaxParameterfv'

  import glu,\
	 gluBeginCurve,'gluBeginCurve',\
	 gluBeginPolygon,'gluBeginPolygon',\
	 gluBeginSurface,'gluBeginSurface',\
	 gluBeginTrim,'gluBeginTrim',\
	 gluBuild1DMipmaps,'gluBuild1DMipmaps',\
	 gluBuild2DMipmaps,'gluBuild2DMipmaps',\
	 gluCylinder,'gluCylinder',\
	 gluDeleteNurbsRenderer,'gluDeleteNurbsRenderer',\
	 gluDeleteQuadric,'gluDeleteQuadric',\
	 gluDeleteTess,'gluDeleteTess',\
	 gluDisk,'gluDisk',\
	 gluEndCurve,'gluEndCurve',\
	 gluEndPolygon,'gluEndPolygon',\
	 gluEndSurface,'gluEndSurface',\
	 gluEndTrim,'gluEndTrim',\
	 gluErrorString,'gluErrorString',\
	 gluGetNurbsProperty,'gluGetNurbsProperty',\
	 gluGetString,'gluGetString',\
	 gluGetTessProperty,'gluGetTessProperty',\
	 gluLoadSamplingMatrices,'gluLoadSamplingMatrices',\
	 gluLookAt,'gluLookAt',\
	 gluNewNurbsRenderer,'gluNewNurbsRenderer',\
	 gluNewQuadric,'gluNewQuadric',\
	 gluNewTess,'gluNewTess',\
	 gluNextContour,'gluNextContour',\
	 gluNurbsCallback,'gluNurbsCallback',\
	 gluNurbsCurve,'gluNurbsCurve',\
	 gluNurbsProperty,'gluNurbsProperty',\
	 gluNurbsSurface,'gluNurbsSurface',\
	 gluOrtho2D,'gluOrtho2D',\
	 gluPartialDisk,'gluPartialDisk',\
	 gluPerspective,'gluPerspective',\
	 gluPickMatrix,'gluPickMatrix',\
	 gluProject,'gluProject',\
	 gluPwlCurve,'gluPwlCurve',\
	 gluQuadricCallback,'gluQuadricCallback',\
	 gluQuadricDrawStyle,'gluQuadricDrawStyle',\
	 gluQuadricNormals,'gluQuadricNormals',\
	 gluQuadricOrientation,'gluQuadricOrientation',\
	 gluQuadricTexture,'gluQuadricTexture',\
	 gluScaleImage,'gluScaleImage',\
	 gluSphere,'gluSphere',\
	 gluTessBeginContour,'gluTessBeginContour',\
	 gluTessBeginPolygon,'gluTessBeginPolygon',\
	 gluTessCallback,'gluTessCallback',\
	 gluTessEndContour,'gluTessEndContour',\
	 gluTessEndPolygon,'gluTessEndPolygon',\
	 gluTessNormal,'gluTessNormal',\
	 gluTessProperty,'gluTessProperty',\
	 gluTessVertex,'gluTessVertex',\
	 gluUnProject,'gluUnProject'
