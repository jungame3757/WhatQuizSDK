mergeInto(LibraryManager.library, {
  
  OpenURL: function(urlPtr) {
    try {
      var url = UTF8ToString(urlPtr);
      
      // 새 창에서 URL 열기
      var opened = window.open(url, '_blank');
      
      // 팝업 차단 확인
      if (!opened) {
        console.error("팝업이 차단되었거나 URL을 열 수 없습니다.");
      }
    } catch (error) {
      console.error("OpenURL 오류:", error);
    }
  },

  OpenURLInSameWindow: function (url) {
    var parsedUrl = UTF8ToString(url);
    window.location.replace(parsedUrl);
  },
  
  GetQRCodeByURL: function (urlPtr, sizePtr, callbackGameObjectPtr, callbackMethodPtr) {
    try {
      var url = UTF8ToString(urlPtr);
      var size = parseInt(UTF8ToString(sizePtr));
      var callbackGameObject = UTF8ToString(callbackGameObjectPtr);
      var callbackMethod = UTF8ToString(callbackMethodPtr);
      
      console.log("QR 코드 URL 생성 시작: " + url);
      
      // QR 코드 생성을 위한 안정적인 서비스 URL 생성
      // QR 코드 생성 서비스를 사용하여 이미지 URL 획득
      // API 키가 필요없는 QR 코드 서비스를 사용
      
      // 옵션 1: QR Server API (프리티어로 사용 가능)
      // var qrImageUrl = "https://api.qrserver.com/v1/create-qr-code/?size=" + size + "x" + size + "&data=" + encodeURIComponent(url);
      
      // 옵션 2: GoQR.me API (무료 서비스)
      var qrImageUrl = "https://api.qrserver.com/v1/create-qr-code/?size=" + size + "x" + size + "&data=" + encodeURIComponent(url);
      
      console.log("QR 코드 이미지 URL 생성 완료: " + qrImageUrl);
      
      // Unity로 QR 코드 이미지 URL 직접 전달
      unityInstance.SendMessage(callbackGameObject, callbackMethod, qrImageUrl);
      
    } catch (error) {
      console.error("GetQRCodeByURL 오류:", error);
      unityInstance.SendMessage(callbackGameObject, callbackMethod, "");
    }
  },
  
  CopyURLToClipboard: function (textPtr) {
    try {
      var text = UTF8ToString(textPtr);
      console.log("클립보드에 복사: " + text);
      
      // 다양한 브라우저 환경에서 클립보드 복사를 지원하기 위한 방법들
      
      // 방법 1: 새로운 Clipboard API 사용 (최신 브라우저)
      if (navigator.clipboard && navigator.clipboard.writeText) {
        navigator.clipboard.writeText(text)
          .then(function() {
            console.log("클립보드에 복사 성공 (Clipboard API)");
          })
          .catch(function(err) {
            console.error("클립보드 복사 실패 (Clipboard API): ", err);
            // 실패 시 대체 방법 사용
            copyUsingFallback(text);
          });
        return;
      }
      
      // 방법 2: 구형 브라우저를 위한 대체 방법
      copyUsingFallback(text);
      
      function copyUsingFallback(text) {
        // textarea 요소 생성
        var textArea = document.createElement("textarea");
        textArea.value = text;
        
        // 화면에 보이지 않게 설정
        textArea.style.position = "fixed";
        textArea.style.left = "-999999px";
        textArea.style.top = "-999999px";
        document.body.appendChild(textArea);
        
        // 텍스트 선택 및 복사
        textArea.focus();
        textArea.select();
        
        var success = false;
        try {
          // 복사 명령 실행
          success = document.execCommand("copy");
          console.log(success ? "클립보드에 복사 성공 (execCommand)" : "클립보드에 복사 실패 (execCommand)");
        } catch (err) {
          console.error("클립보드 복사 중 오류: ", err);
        }
        
        // 임시 요소 제거
        document.body.removeChild(textArea);
      }
    } catch (error) {
      console.error("클립보드 복사 중 오류: ", error);
    }
  }
}); 