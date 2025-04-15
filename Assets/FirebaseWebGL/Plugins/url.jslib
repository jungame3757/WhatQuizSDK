mergeInto(LibraryGLCore, {
  OpenUrlWithToken: function(urlPtr, tokenPtr) {
    var url = Pointer_stringify(urlPtr);
    var token = Pointer_stringify(tokenPtr);
    
    // 토큰을 URL 파라미터로 추가 (URL에 이미 파라미터가 있는지 확인)
    var connector = url.indexOf('?') !== -1 ? '&' : '?';
    var urlWithToken = url + connector + 'token=' + encodeURIComponent(token);
    
    // 새 창에서 URL 열기
    window.open(urlWithToken, '_blank');
  },
  
  OpenURL: function(urlPtr) {
    var url = Pointer_stringify(urlPtr);
    
    // 새 창에서 URL 열기
    window.open(url, '_blank');
  }
}); 