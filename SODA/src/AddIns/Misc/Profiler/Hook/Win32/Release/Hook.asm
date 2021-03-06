; Listing generated by Microsoft (R) Optimizing Compiler Version 15.00.30729.01 

	TITLE	c:\Local\SharpDevelop_3.2.0.5777_Source\src\AddIns\Misc\Profiler\Hook\Hook.cpp
	.686P
	.XMM
	include listing.inc
	.model	flat

INCLUDELIB OLDNAMES

PUBLIC	?getThreadLocalData@@YAPAUThreadLocalData@@XZ	; getThreadLocalData
PUBLIC	??0IClassFactory@@QAE@XZ			; IClassFactory::IClassFactory
PUBLIC	?AddRef@CProfilerFactory@@UAGKXZ		; CProfilerFactory::AddRef
PUBLIC	?Release@CProfilerFactory@@UAGKXZ		; CProfilerFactory::Release
PUBLIC	_IsEqualGUID
PUBLIC	?QueryInterface@CProfilerFactory@@UAGJABU_GUID@@PAPAX@Z ; CProfilerFactory::QueryInterface
PUBLIC	?CreateInstance@CProfilerFactory@@UAGJPAUIUnknown@@ABU_GUID@@PAPAX@Z ; CProfilerFactory::CreateInstance
PUBLIC	?LockServer@CProfilerFactory@@UAGJH@Z		; CProfilerFactory::LockServer
PUBLIC	??0CProfilerFactory@@QAE@XZ			; CProfilerFactory::CProfilerFactory
PUBLIC	?tls_index@@3KA					; tls_index
PUBLIC	?g_cRefThisDll@@3JA				; g_cRefThisDll
PUBLIC	?g_module@@3PAXA				; g_module
PUBLIC	?allThreadLocalDatas@@3PAVLightweightList@@A	; allThreadLocalDatas
PUBLIC	??_7CProfilerFactory@@6B@			; CProfilerFactory::`vftable'
EXTRN	__imp__TlsAlloc@0:PROC
EXTRN	__imp__TlsSetValue@8:PROC
EXTRN	__imp__TlsGetValue@4:PROC
EXTRN	_IID_IUnknown:BYTE
EXTRN	_IID_IClassFactory:BYTE
?tls_index@@3KA DD 01H DUP (?)				; tls_index
?g_cRefThisDll@@3JA DD 01H DUP (?)			; g_cRefThisDll
?g_module@@3PAXA DD 01H DUP (?)				; g_module
?allThreadLocalDatas@@3PAVLightweightList@@A DD 01H DUP (?) ; allThreadLocalDatas
;	COMDAT ??_7CProfilerFactory@@6B@
CONST	SEGMENT
??_7CProfilerFactory@@6B@ DD FLAT:?QueryInterface@CProfilerFactory@@UAGJABU_GUID@@PAPAX@Z ; CProfilerFactory::`vftable'
	DD	FLAT:?AddRef@CProfilerFactory@@UAGKXZ
	DD	FLAT:?Release@CProfilerFactory@@UAGKXZ
	DD	FLAT:?CreateInstance@CProfilerFactory@@UAGJPAUIUnknown@@ABU_GUID@@PAPAX@Z
	DD	FLAT:?LockServer@CProfilerFactory@@UAGJH@Z
_CLSID_Profiler DD 0e7e2c111H
	DW	03471H
	DW	04ac7H
	DB	0b2H
	DB	078H
	DB	011H
	DB	0f4H
	DB	0c2H
	DB	06eH
	DB	0dbH
	DB	0cfH
__bad_alloc_Message DD FLAT:??_C@_0P@GHFPNOJB@bad?5allocation?$AA@
PUBLIC	_DllCanUnloadNow@0
; Function compile flags: /Ogtpy
; File c:\local\sharpdevelop_3.2.0.5777_source\src\addins\misc\profiler\hook\hook.cpp
;	COMDAT _DllCanUnloadNow@0
_TEXT	SEGMENT
_DllCanUnloadNow@0 PROC					; COMDAT

; 85   :     return (g_cRefThisDll == 0 ? S_OK : S_FALSE);

	xor	eax, eax
	cmp	DWORD PTR ?g_cRefThisDll@@3JA, eax	; g_cRefThisDll
	setne	al

; 86   : }

	ret	0
_DllCanUnloadNow@0 ENDP
; Function compile flags: /Ogtpy
; File c:\local\sharpdevelop_3.2.0.5777_source\src\addins\misc\profiler\hook\profilerfactory.h
_TEXT	ENDS
;	COMDAT ?LockServer@CProfilerFactory@@UAGJH@Z
_TEXT	SEGMENT
_this$ = 8						; size = 4
___formal$ = 12						; size = 4
?LockServer@CProfilerFactory@@UAGJH@Z PROC		; CProfilerFactory::LockServer, COMDAT

; 46   : 	STDMETHODIMP LockServer(BOOL) {	return S_OK; }  // not implemented

	xor	eax, eax
	ret	8
?LockServer@CProfilerFactory@@UAGJH@Z ENDP		; CProfilerFactory::LockServer
; Function compile flags: /Ogtpy
_TEXT	ENDS
;	COMDAT ?Release@CProfilerFactory@@UAGKXZ
_TEXT	SEGMENT
_this$ = 8						; size = 4
?Release@CProfilerFactory@@UAGKXZ PROC			; CProfilerFactory::Release, COMDAT

; 24   : 		return 1; // Singleton

	mov	eax, 1

; 25   : 	}

	ret	4
?Release@CProfilerFactory@@UAGKXZ ENDP			; CProfilerFactory::Release
; Function compile flags: /Ogtpy
_TEXT	ENDS
;	COMDAT ?AddRef@CProfilerFactory@@UAGKXZ
_TEXT	SEGMENT
_this$ = 8						; size = 4
?AddRef@CProfilerFactory@@UAGKXZ PROC			; CProfilerFactory::AddRef, COMDAT

; 19   : 		return 1; // Singleton

	mov	eax, 1

; 20   : 	}

	ret	4
?AddRef@CProfilerFactory@@UAGKXZ ENDP			; CProfilerFactory::AddRef
; Function compile flags: /Ogtpy
; File c:\local\sharpdevelop_3.2.0.5777_source\src\addins\misc\profiler\hook\global.h
_TEXT	ENDS
;	COMDAT ?getThreadLocalData@@YAPAUThreadLocalData@@XZ
_TEXT	SEGMENT
?getThreadLocalData@@YAPAUThreadLocalData@@XZ PROC	; getThreadLocalData, COMDAT

; 46   : 	return (ThreadLocalData*)TlsGetValue(tls_index);

	mov	eax, DWORD PTR ?tls_index@@3KA		; tls_index
	push	eax
	call	DWORD PTR __imp__TlsGetValue@4

; 47   : }

	ret	0
?getThreadLocalData@@YAPAUThreadLocalData@@XZ ENDP	; getThreadLocalData
; Function compile flags: /Ogtpy
; File c:\program files\microsoft sdks\windows\v7.0\include\guiddef.h
_TEXT	ENDS
;	COMDAT _IsEqualGUID
_TEXT	SEGMENT
_IsEqualGUID PROC					; COMDAT
; _rguid1$ = edx
; _rguid2$ = ecx

; 161  :     return !memcmp(&rguid1, &rguid2, sizeof(GUID));

	mov	eax, 16					; 00000010H
	push	esi
$LL4@IsEqualGUI:
	mov	esi, DWORD PTR [edx]
	cmp	esi, DWORD PTR [ecx]
	jne	SHORT $LN5@IsEqualGUI
	sub	eax, 4
	add	ecx, 4
	add	edx, 4
	cmp	eax, 4
	jae	SHORT $LL4@IsEqualGUI
	xor	eax, eax
	xor	edx, edx
	test	eax, eax
	sete	dl
	pop	esi
	mov	eax, edx

; 162  : }

	ret	0
$LN5@IsEqualGUI:

; 161  :     return !memcmp(&rguid1, &rguid2, sizeof(GUID));

	movzx	eax, BYTE PTR [edx]
	movzx	esi, BYTE PTR [ecx]
	sub	eax, esi
	jne	SHORT $LN7@IsEqualGUI
	movzx	eax, BYTE PTR [edx+1]
	movzx	esi, BYTE PTR [ecx+1]
	sub	eax, esi
	jne	SHORT $LN7@IsEqualGUI
	movzx	eax, BYTE PTR [edx+2]
	movzx	esi, BYTE PTR [ecx+2]
	sub	eax, esi
	jne	SHORT $LN7@IsEqualGUI
	movzx	eax, BYTE PTR [edx+3]
	movzx	ecx, BYTE PTR [ecx+3]
	sub	eax, ecx
$LN7@IsEqualGUI:
	sar	eax, 31					; 0000001fH
	or	eax, 1
	xor	edx, edx
	test	eax, eax
	sete	dl
	pop	esi
	mov	eax, edx

; 162  : }

	ret	0
_IsEqualGUID ENDP
; Function compile flags: /Ogtpy
;	COMDAT ??0IClassFactory@@QAE@XZ
_TEXT	SEGMENT
??0IClassFactory@@QAE@XZ PROC				; IClassFactory::IClassFactory, COMDAT
; _this$ = eax
	ret	0
??0IClassFactory@@QAE@XZ ENDP				; IClassFactory::IClassFactory
; Function compile flags: /Ogtpy
_TEXT	ENDS
;	COMDAT ??0CProfilerFactory@@QAE@XZ
_TEXT	SEGMENT
??0CProfilerFactory@@QAE@XZ PROC			; CProfilerFactory::CProfilerFactory, COMDAT
; _this$ = eax
	mov	DWORD PTR [eax], OFFSET ??_7CProfilerFactory@@6B@
	ret	0
??0CProfilerFactory@@QAE@XZ ENDP			; CProfilerFactory::CProfilerFactory
_TEXT	ENDS
PUBLIC	_DllGetClassObject@12
; Function compile flags: /Ogtpy
; File c:\local\sharpdevelop_3.2.0.5777_source\src\addins\misc\profiler\hook\hook.cpp
;	COMDAT _DllGetClassObject@12
_TEXT	SEGMENT
_rclsid$ = 8						; size = 4
_riid$ = 12						; size = 4
_ppvOut$ = 16						; size = 4
_DllGetClassObject@12 PROC				; COMDAT

; 71   : 	// MessageBox(nullptr, TEXT("DllGetClassObject"), TEXT("DllGetClassObject"), MB_OK);
; 72   : 
; 73   : 	*ppvOut = nullptr;
; 74   :     if (IsEqualIID(rclsid, CLSID_Profiler))

	mov	edx, DWORD PTR _rclsid$[esp-4]
	push	esi
	mov	esi, DWORD PTR _ppvOut$[esp]
	mov	ecx, OFFSET _CLSID_Profiler
	mov	DWORD PTR [esi], 0
	call	_IsEqualGUID
	test	eax, eax
	je	SHORT $LN1@DllGetClas

; 75   :     {
; 76   :        // declare a classfactory for CProfiler class 
; 77   :        CProfilerFactory *pcf = new CProfilerFactory;

	push	4
	call	??2@YAPAXI@Z				; operator new
	add	esp, 4
	test	eax, eax
	je	SHORT $LN4@DllGetClas

; 78   :        return pcf->QueryInterface(riid,ppvOut);

	mov	edx, DWORD PTR _riid$[esp]
	push	esi
	mov	DWORD PTR [eax], OFFSET ??_7CProfilerFactory@@6B@
	mov	ecx, DWORD PTR [eax]
	push	edx
	push	eax
	mov	eax, DWORD PTR [ecx]
	call	eax
	pop	esi

; 81   : }

	ret	12					; 0000000cH
$LN4@DllGetClas:

; 78   :        return pcf->QueryInterface(riid,ppvOut);

	mov	edx, DWORD PTR _riid$[esp]
	xor	eax, eax
	mov	ecx, DWORD PTR [eax]
	push	esi
	push	edx
	push	eax
	mov	eax, DWORD PTR [ecx]
	call	eax
	pop	esi

; 81   : }

	ret	12					; 0000000cH
$LN1@DllGetClas:

; 79   :     }
; 80   :     return CLASS_E_CLASSNOTAVAILABLE;

	mov	eax, -2147221231			; 80040111H
	pop	esi

; 81   : }

	ret	12					; 0000000cH
_DllGetClassObject@12 ENDP
_TEXT	ENDS
PUBLIC	_DllMain@12
; Function compile flags: /Ogtpy
;	COMDAT _DllMain@12
_TEXT	SEGMENT
_hModule$ = 8						; size = 4
_ul_reason_for_call$ = 12				; size = 4
___formal$ = 16						; size = 4
_DllMain@12 PROC					; COMDAT

; 34   : 	switch (ul_reason_for_call)

	mov	eax, DWORD PTR _ul_reason_for_call$[esp-4]
	sub	eax, 1
	je	SHORT $LN5@DllMain
	sub	eax, 1
	je	SHORT $LN4@DllMain
	sub	eax, 1
	jne	SHORT $LN13@DllMain

; 47   : 			//AttachToThread();
; 48   : 			break;
; 49   : 		}
; 50   : 
; 51   : 		case DLL_THREAD_DETACH: {
; 52   : 			DebugWriteLine(L"DLL_THREAD_DETACH");
; 53   : 			ThreadLocalData *data = getThreadLocalData();

	mov	eax, DWORD PTR ?tls_index@@3KA		; tls_index
	push	eax
	call	DWORD PTR __imp__TlsGetValue@4
	mov	ecx, eax

; 54   : 			DetachFromThread(data);

	call	?DetachFromThread@@YAXPAUThreadLocalData@@@Z ; DetachFromThread

; 55   : 			if (data != nullptr)

	test	ecx, ecx
	je	SHORT $LN13@DllMain
	push	edi

; 56   : 				allThreadLocalDatas->remove(data);

	mov	edi, ecx
	call	?remove@LightweightList@@QAEXPAUThreadLocalData@@@Z ; LightweightList::remove
	pop	edi

; 57   : 			break;
; 58   : 		}
; 59   : 		
; 60   : 		case DLL_PROCESS_DETACH: {
; 61   : 			DebugWriteLine(L"DLL_PROCESS_DETACH");
; 62   : 			break;
; 63   : 		}
; 64   : 	}
; 65   : 
; 66   : 	return TRUE;

	mov	eax, 1

; 67   : }

	ret	12					; 0000000cH
$LN4@DllMain:

; 41   : 			//AttachToThread();
; 42   : 			break;
; 43   : 
; 44   : 		case DLL_THREAD_ATTACH: {
; 45   : 			DebugWriteLine(L"DLL_THREAD_ATTACH");
; 46   : 			TlsSetValue(tls_index, nullptr);

	mov	ecx, DWORD PTR ?tls_index@@3KA		; tls_index
	push	0
	push	ecx
	call	DWORD PTR __imp__TlsSetValue@8

; 57   : 			break;
; 58   : 		}
; 59   : 		
; 60   : 		case DLL_PROCESS_DETACH: {
; 61   : 			DebugWriteLine(L"DLL_PROCESS_DETACH");
; 62   : 			break;
; 63   : 		}
; 64   : 	}
; 65   : 
; 66   : 	return TRUE;

	mov	eax, 1

; 67   : }

	ret	12					; 0000000cH
$LN5@DllMain:

; 35   : 	{
; 36   : 		case DLL_PROCESS_ATTACH: 
; 37   : 			DebugWriteLine(L"DLL_PROCESS_ATTACH");
; 38   : 			g_module = hModule;

	mov	edx, DWORD PTR _hModule$[esp-4]
	mov	DWORD PTR ?g_module@@3PAXA, edx		; g_module

; 39   : 			tls_index = TlsAlloc();

	call	DWORD PTR __imp__TlsAlloc@0

; 40   : 			TlsSetValue(tls_index, nullptr);

	push	0
	push	eax
	mov	DWORD PTR ?tls_index@@3KA, eax		; tls_index
	call	DWORD PTR __imp__TlsSetValue@8
$LN13@DllMain:

; 57   : 			break;
; 58   : 		}
; 59   : 		
; 60   : 		case DLL_PROCESS_DETACH: {
; 61   : 			DebugWriteLine(L"DLL_PROCESS_DETACH");
; 62   : 			break;
; 63   : 		}
; 64   : 	}
; 65   : 
; 66   : 	return TRUE;

	mov	eax, 1

; 67   : }

	ret	12					; 0000000cH
_DllMain@12 ENDP
; Function compile flags: /Ogtpy
; File c:\local\sharpdevelop_3.2.0.5777_source\src\addins\misc\profiler\hook\profilerfactory.h
_TEXT	ENDS
;	COMDAT ?CreateInstance@CProfilerFactory@@UAGJPAUIUnknown@@ABU_GUID@@PAPAX@Z
_TEXT	SEGMENT
_this$ = 8						; size = 4
_pUnkOuter$ = 12					; size = 4
_riid$ = 16						; size = 4
_ppvObj$ = 20						; size = 4
?CreateInstance@CProfilerFactory@@UAGJPAUIUnknown@@ABU_GUID@@PAPAX@Z PROC ; CProfilerFactory::CreateInstance, COMDAT

; 39   : 		*ppvObj = nullptr;
; 40   : 		if (pUnkOuter)

	cmp	DWORD PTR _pUnkOuter$[esp-4], 0
	push	esi
	mov	esi, DWORD PTR _ppvObj$[esp]
	mov	DWORD PTR [esi], 0
	je	SHORT $LN1@CreateInst

; 41   :     		return CLASS_E_NOAGGREGATION;

	mov	eax, -2147221232			; 80040110H
	pop	esi

; 43   : 		return hr;
; 44   : 	}

	ret	16					; 00000010H
$LN1@CreateInst:
	push	edi

; 42   : 		HRESULT hr = profiler.QueryInterface(riid, ppvObj);

	mov	edi, DWORD PTR _riid$[esp+4]
	mov	ecx, OFFSET _IID_IUnknown
	mov	edx, edi
	mov	DWORD PTR [esi], 0
	call	_IsEqualGUID
	test	eax, eax
	jne	SHORT $LN4@CreateInst
	mov	ecx, OFFSET _IID_ICorProfilerCallback
	mov	edx, edi
	call	_IsEqualGUID
	test	eax, eax
	jne	SHORT $LN4@CreateInst
	mov	ecx, OFFSET _IID_ICorProfilerCallback2
	mov	edx, edi
	call	_IsEqualGUID
	test	eax, eax
	jne	SHORT $LN4@CreateInst
	pop	edi
	mov	eax, -2147467262			; 80004002H
	pop	esi

; 43   : 		return hr;
; 44   : 	}

	ret	16					; 00000010H

; 42   : 		HRESULT hr = profiler.QueryInterface(riid, ppvObj);

$LN4@CreateInst:
	pop	edi
	mov	DWORD PTR [esi], OFFSET ?profiler@@3VCProfiler@@A ; profiler
	xor	eax, eax
	pop	esi

; 43   : 		return hr;
; 44   : 	}

	ret	16					; 00000010H
?CreateInstance@CProfilerFactory@@UAGJPAUIUnknown@@ABU_GUID@@PAPAX@Z ENDP ; CProfilerFactory::CreateInstance
; Function compile flags: /Ogtpy
_TEXT	ENDS
;	COMDAT ?QueryInterface@CProfilerFactory@@UAGJABU_GUID@@PAPAX@Z
_TEXT	SEGMENT
_this$ = 8						; size = 4
_riid$ = 12						; size = 4
_ppv$ = 16						; size = 4
?QueryInterface@CProfilerFactory@@UAGJABU_GUID@@PAPAX@Z PROC ; CProfilerFactory::QueryInterface, COMDAT

; 28   : 	{

	push	esi

; 29   : 		*ppv = nullptr;
; 30   : 		if(IsEqualIID(riid,IID_IUnknown) || IsEqualIID(riid,IID_IClassFactory)) {

	mov	esi, DWORD PTR _riid$[esp]
	push	edi
	mov	edi, DWORD PTR _ppv$[esp+4]
	mov	ecx, OFFSET _IID_IUnknown
	mov	edx, esi
	mov	DWORD PTR [edi], 0
	call	_IsEqualGUID
	test	eax, eax
	jne	SHORT $LN1@QueryInter@2
	mov	ecx, OFFSET _IID_IClassFactory
	mov	edx, esi
	call	_IsEqualGUID
	test	eax, eax
	jne	SHORT $LN1@QueryInter@2
	pop	edi

; 33   : 		}
; 34   : 		return E_NOINTERFACE;

	mov	eax, -2147467262			; 80004002H
	pop	esi

; 35   : 	}

	ret	12					; 0000000cH
$LN1@QueryInter@2:

; 31   : 			*ppv = this;

	mov	eax, DWORD PTR _this$[esp+4]
	mov	DWORD PTR [edi], eax
	pop	edi

; 32   : 			return S_OK;

	xor	eax, eax
	pop	esi

; 35   : 	}

	ret	12					; 0000000cH
?QueryInterface@CProfilerFactory@@UAGJABU_GUID@@PAPAX@Z ENDP ; CProfilerFactory::QueryInterface
PUBLIC	?AttachToThread@@YAPAUThreadLocalData@@XZ	; AttachToThread
; Function compile flags: /Ogtpy
; File c:\local\sharpdevelop_3.2.0.5777_source\src\addins\misc\profiler\hook\hook.cpp
;	COMDAT ?AttachToThread@@YAPAUThreadLocalData@@XZ
_TEXT	SEGMENT
?AttachToThread@@YAPAUThreadLocalData@@XZ PROC		; AttachToThread, COMDAT

; 23   : {

	push	esi

; 24   : 	ThreadLocalData *data = allThreadLocalDatas->add();

	call	?add@LightweightList@@QAEPAUThreadLocalData@@XZ ; LightweightList::add
	mov	esi, eax

; 25   : 	TlsSetValue(tls_index, data);

	mov	eax, DWORD PTR ?tls_index@@3KA		; tls_index
	push	esi
	push	eax
	call	DWORD PTR __imp__TlsSetValue@8

; 26   : 	return data;

	mov	eax, esi
	pop	esi

; 27   : }

	ret	0
?AttachToThread@@YAPAUThreadLocalData@@XZ ENDP		; AttachToThread
END
