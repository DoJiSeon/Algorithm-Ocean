mergeInto(LibraryManager.library, {
  $AOYT_CreateBridge: function () {
    const players = {};
    let apiReady = false;
    let apiPromise = null;

    function loadApi() {
      if (apiReady) {
        return Promise.resolve();
      }

      if (apiPromise) {
        return apiPromise;
      }

      apiPromise = new Promise(function (resolve) {
        const previousReady = window.onYouTubeIframeAPIReady;
        window.onYouTubeIframeAPIReady = function () {
          apiReady = true;
          if (typeof previousReady === "function") {
            previousReady();
          }
          resolve();
        };

        if (window.YT && window.YT.Player) {
          apiReady = true;
          resolve();
          return;
        }

        const script = document.createElement("script");
        script.src = "https://www.youtube.com/iframe_api";
        document.head.appendChild(script);
      });

      return apiPromise;
    }

    function getUnityCanvas() {
      return document.querySelector("#unity-canvas") ||
        document.querySelector("canvas") ||
        document.body;
    }

    function installFullscreenPatch() {
      const canvas = getUnityCanvas();
      if (!canvas || canvas === document.body || canvas.__aoYoutubeFullscreenPatched) {
        return;
      }

      const host = canvas.parentElement;
      if (!host) {
        return;
      }

      canvas.__aoYoutubeFullscreenPatched = true;
      canvas.__aoYoutubeFullscreenStyles = {
        hostWidth: host.style.width,
        hostHeight: host.style.height,
        hostPosition: host.style.position,
        hostOverflow: host.style.overflow,
        hostBackground: host.style.background,
        canvasWidth: canvas.style.width,
        canvasHeight: canvas.style.height,
        canvasDisplay: canvas.style.display
      };

      function applyFullscreenStyles(active) {
        const styles = canvas.__aoYoutubeFullscreenStyles;
        if (!styles) {
          return;
        }

        if (active) {
          host.style.width = "100vw";
          host.style.height = "100vh";
          host.style.position = "relative";
          host.style.overflow = "hidden";
          host.style.background = "black";
          canvas.style.width = "100%";
          canvas.style.height = "100%";
          canvas.style.display = "block";
          return;
        }

        host.style.width = styles.hostWidth;
        host.style.height = styles.hostHeight;
        host.style.position = styles.hostPosition;
        host.style.overflow = styles.hostOverflow;
        host.style.background = styles.hostBackground;
        canvas.style.width = styles.canvasWidth;
        canvas.style.height = styles.canvasHeight;
        canvas.style.display = styles.canvasDisplay;
      }

      function requestHostFullscreen(requestHost) {
        applyFullscreenStyles(true);
        const result = requestHost();
        if (result && typeof result.catch === "function") {
          result.catch(function () {
            applyFullscreenStyles(false);
          });
        }
        return result;
      }

      document.addEventListener("fullscreenchange", function () {
        applyFullscreenStyles(document.fullscreenElement === host);
      });

      document.addEventListener("webkitfullscreenchange", function () {
        applyFullscreenStyles(document.webkitFullscreenElement === host);
      });

      const requestFullscreen = canvas.requestFullscreen;
      if (requestFullscreen) {
        canvas.requestFullscreen = function (options) {
          if (host.requestFullscreen) {
            return requestHostFullscreen(function () {
              return host.requestFullscreen(options);
            });
          }
          return requestFullscreen.call(canvas, options);
        };
      }

      const webkitRequestFullscreen = canvas.webkitRequestFullscreen;
      if (webkitRequestFullscreen) {
        canvas.webkitRequestFullscreen = function () {
          if (host.webkitRequestFullscreen) {
            return requestHostFullscreen(function () {
              return host.webkitRequestFullscreen();
            });
          }
          return webkitRequestFullscreen.call(canvas);
        };
      }

      const mozRequestFullScreen = canvas.mozRequestFullScreen;
      if (mozRequestFullScreen) {
        canvas.mozRequestFullScreen = function () {
          if (host.mozRequestFullScreen) {
            return requestHostFullscreen(function () {
              return host.mozRequestFullScreen();
            });
          }
          return mozRequestFullScreen.call(canvas);
        };
      }

      const msRequestFullscreen = canvas.msRequestFullscreen;
      if (msRequestFullscreen) {
        canvas.msRequestFullscreen = function () {
          if (host.msRequestFullscreen) {
            return requestHostFullscreen(function () {
              return host.msRequestFullscreen();
            });
          }
          return msRequestFullscreen.call(canvas);
        };
      }
    }

    function getOverlayHost() {
      installFullscreenPatch();

      const fullscreenElement = document.fullscreenElement ||
        document.webkitFullscreenElement ||
        document.mozFullScreenElement ||
        document.msFullscreenElement;

      if (fullscreenElement && fullscreenElement.tagName !== "CANVAS") {
        return fullscreenElement;
      }

      const canvas = getUnityCanvas();
      return canvas.parentElement || document.body;
    }

    function createContainer(objectName) {
      const container = document.createElement("div");
      container.id = "ao-youtube-" + objectName;
      container.style.position = "fixed";
      container.style.overflow = "hidden";
      container.style.backgroundColor = "black";
      container.style.zIndex = "2147483647";
      container.style.display = "none";
      container.style.pointerEvents = "auto";
      container.style.color = "white";

      const playerElement = document.createElement("iframe");
      playerElement.id = container.id + "-player";
      playerElement.style.width = "100%";
      playerElement.style.height = "100%";
      playerElement.style.border = "0";
      playerElement.allow = "accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share";
      playerElement.allowFullscreen = true;
      container.appendChild(playerElement);

      getOverlayHost().appendChild(container);
      return { container, playerElement };
    }

    function getCanvasRect() {
      const fullscreenElement = document.fullscreenElement ||
        document.webkitFullscreenElement ||
        document.mozFullScreenElement ||
        document.msFullscreenElement;

      if (fullscreenElement) {
        return {
          left: 0,
          top: 0,
          width: window.innerWidth,
          height: window.innerHeight
        };
      }

      const canvas = getUnityCanvas();
      return canvas.getBoundingClientRect();
    }

    function sendMessage(objectName, method, value) {
      if (typeof SendMessage === "function") {
        SendMessage(objectName, method, value || "");
      }
    }

    function report(objectName, message) {
      console.log("[AOYT] " + message);
      sendMessage(objectName, "OnYouTubePlayerState", message);
    }

    function stateName(state) {
      switch (state) {
        case YT.PlayerState.ENDED: return "ended";
        case YT.PlayerState.PLAYING: return "playing";
        case YT.PlayerState.PAUSED: return "paused";
        case YT.PlayerState.BUFFERING: return "buffering";
        case YT.PlayerState.CUED: return "cued";
        default: return "unstarted";
      }
    }

    function ensurePlayer(objectName, options) {
      installFullscreenPatch();

      if (players[objectName]) {
        return players[objectName];
      }

      const elements = createContainer(objectName);
      const entry = {
        objectName: objectName,
        container: elements.container,
        iframe: elements.playerElement,
        options: options,
        videoId: "",
        autoplay: false,
        ready: false,
        visible: false,
        rect: { x: 0, y: 0, width: 0, height: 0 }
      };

      players[objectName] = entry;
      report(objectName, "created iframe container");
      return entry;
    }

    function buildPlayer(entry) {
      if (entry.player || !window.YT || !window.YT.Player) {
        return;
      }

      entry.player = new YT.Player(entry.playerElement.id, {
        width: "100%",
        height: "100%",
        videoId: entry.videoId,
        playerVars: {
          autoplay: 0,
          controls: entry.options.controls ? 1 : 0,
          playsinline: 1,
          rel: 0,
          loop: entry.options.loop ? 1 : 0,
          mute: entry.options.muted ? 1 : 0,
          playlist: entry.options.loop && entry.videoId ? entry.videoId : undefined
        },
        events: {
          onReady: function (event) {
            entry.ready = true;
            if (entry.options.muted) {
              event.target.mute();
            }
            loadEntryVideo(entry);
            sendMessage(entry.objectName, "OnYouTubePlayerReady", "");
          },
          onStateChange: function (event) {
            sendMessage(entry.objectName, "OnYouTubePlayerState", stateName(event.data));
          },
          onError: function (event) {
            sendMessage(entry.objectName, "OnYouTubePlayerError", String(event.data));
          }
        }
      });
    }

    function loadEntryVideo(entry) {
      if (!entry.iframe || !entry.videoId) {
        report(entry.objectName, "load skipped: iframe or videoId missing");
        return;
      }

      const params = new URLSearchParams();
      params.set("playsinline", "1");
      params.set("rel", "0");
      params.set("controls", entry.options.controls ? "1" : "0");
      params.set("autoplay", entry.autoplay ? "1" : "0");
      params.set("mute", entry.options.muted ? "1" : "0");
      params.set("loop", entry.options.loop ? "1" : "0");
      params.set("enablejsapi", "1");
      params.set("origin", window.location.origin);

      if (entry.options.loop) {
        params.set("playlist", entry.videoId);
      }

      entry.iframe.src = "https://www.youtube.com/embed/" + encodeURIComponent(entry.videoId) + "?" + params.toString();
      report(entry.objectName, "iframe src set: " + entry.videoId);
      sendMessage(entry.objectName, "OnYouTubePlayerReady", "");
    }

    function applyRect(entry) {
      const host = getOverlayHost();
      if (entry.container.parentElement !== host) {
        host.appendChild(entry.container);
      }

      const canvasRect = getCanvasRect();
      const viewportWidth = window.innerWidth || document.documentElement.clientWidth || canvasRect.width;
      const viewportHeight = window.innerHeight || document.documentElement.clientHeight || canvasRect.height;
      let left = canvasRect.left + canvasRect.width * entry.rect.x;
      let top = canvasRect.top + canvasRect.height * entry.rect.y;
      let width = canvasRect.width * entry.rect.width;
      let height = canvasRect.height * entry.rect.height;

      if (width < 32 || height < 32) {
        const fallbackHeight = canvasRect.height * 0.72;
        height = Math.min(640, Math.max(320, fallbackHeight));
        width = height * 9 / 16;
        left = canvasRect.left + (canvasRect.width - width) * 0.5;
        top = canvasRect.top + (canvasRect.height - height) * 0.5;
        report(entry.objectName, "rect fallback applied");
      }

      left = Math.max(0, Math.min(left, viewportWidth - width));
      top = Math.max(0, Math.min(top, viewportHeight - height));

      entry.container.style.left = left + "px";
      entry.container.style.top = top + "px";
      entry.container.style.width = width + "px";
      entry.container.style.height = height + "px";
      report(entry.objectName, "shorts frame rect applied: " + Math.round(left) + "," + Math.round(top) + " " + Math.round(width) + "x" + Math.round(height));
    }

    window.addEventListener("resize", function () {
      Object.keys(players).forEach(function (key) {
        applyRect(players[key]);
      });
    });

    document.addEventListener("fullscreenchange", function () {
      Object.keys(players).forEach(function (key) {
        applyRect(players[key]);
      });
    });

    document.addEventListener("webkitfullscreenchange", function () {
      Object.keys(players).forEach(function (key) {
        applyRect(players[key]);
      });
    });

    window.addEventListener("scroll", function () {
      Object.keys(players).forEach(function (key) {
        applyRect(players[key]);
      });
    }, true);

    installFullscreenPatch();

    return {
      create: function (objectName, options) {
        ensurePlayer(objectName, options);
      },

      loadVideo: function (objectName, videoId, autoplay) {
        const entry = ensurePlayer(objectName, { controls: true, muted: true, loop: false });
        entry.videoId = videoId;
        entry.autoplay = autoplay;
        entry.visible = true;
        entry.container.style.display = "block";
        report(objectName, "load requested: " + videoId + ", visible=" + entry.visible);
        loadEntryVideo(entry);
      },

      setRect: function (objectName, x, y, width, height) {
        const entry = ensurePlayer(objectName, { controls: true, muted: true, loop: false });
        entry.rect = { x: x, y: y, width: width, height: height };
        applyRect(entry);
      },

      play: function (objectName) {
        const entry = players[objectName];
        if (entry) {
          entry.autoplay = true;
          loadEntryVideo(entry);
        }
      },

      pause: function (objectName) {
      },

      stop: function (objectName) {
        const entry = players[objectName];
        if (!entry) {
          return;
        }

        entry.autoplay = false;
        entry.visible = false;
        entry.container.style.display = "none";

        if (entry.iframe) {
          entry.iframe.removeAttribute("src");
          entry.iframe.src = "about:blank";
        }
      },

      setVisible: function (objectName, visible) {
        const entry = players[objectName];
        if (!entry) {
          return;
        }
        entry.visible = visible;
        entry.container.style.display = visible ? "block" : "none";
        applyRect(entry);
      },

      destroy: function (objectName) {
        const entry = players[objectName];
        if (!entry) {
          return;
        }

        entry.container.remove();
        delete players[objectName];
      }
    };
  },

  $AOYT_GetBridge__deps: ["$AOYT_CreateBridge"],
  $AOYT_GetBridge: function () {
    if (!window.AlgorithmOceanYouTube) {
      window.AlgorithmOceanYouTube = AOYT_CreateBridge();
    }
    return window.AlgorithmOceanYouTube;
  },

  AOYT_InstallFullscreenPatch__deps: ["$AOYT_GetBridge"],
  AOYT_InstallFullscreenPatch: function () {
    AOYT_GetBridge();
  },

  AOYT_Create__deps: ["$AOYT_GetBridge"],
  AOYT_Create: function (objectNamePtr, controls, muted, loop) {
    const objectName = UTF8ToString(objectNamePtr);
    AOYT_GetBridge().create(objectName, {
      controls: controls === 1,
      muted: muted === 1,
      loop: loop === 1
    });
  },

  AOYT_LoadVideo__deps: ["$AOYT_GetBridge"],
  AOYT_LoadVideo: function (objectNamePtr, videoIdPtr, autoplay) {
    const objectName = UTF8ToString(objectNamePtr);
    const videoId = UTF8ToString(videoIdPtr);
    AOYT_GetBridge().loadVideo(objectName, videoId, autoplay === 1);
  },

  AOYT_SetRect__deps: ["$AOYT_GetBridge"],
  AOYT_SetRect: function (objectNamePtr, x, y, width, height) {
    const objectName = UTF8ToString(objectNamePtr);
    AOYT_GetBridge().setRect(objectName, x, y, width, height);
  },

  AOYT_Play__deps: ["$AOYT_GetBridge"],
  AOYT_Play: function (objectNamePtr) {
    const objectName = UTF8ToString(objectNamePtr);
    AOYT_GetBridge().play(objectName);
  },

  AOYT_Pause__deps: ["$AOYT_GetBridge"],
  AOYT_Pause: function (objectNamePtr) {
    const objectName = UTF8ToString(objectNamePtr);
    AOYT_GetBridge().pause(objectName);
  },

  AOYT_Stop__deps: ["$AOYT_GetBridge"],
  AOYT_Stop: function (objectNamePtr) {
    const objectName = UTF8ToString(objectNamePtr);
    AOYT_GetBridge().stop(objectName);
  },

  AOYT_SetVisible__deps: ["$AOYT_GetBridge"],
  AOYT_SetVisible: function (objectNamePtr, visible) {
    const objectName = UTF8ToString(objectNamePtr);
    AOYT_GetBridge().setVisible(objectName, visible === 1);
  },

  AOYT_Destroy__deps: ["$AOYT_GetBridge"],
  AOYT_Destroy: function (objectNamePtr) {
    const objectName = UTF8ToString(objectNamePtr);
    AOYT_GetBridge().destroy(objectName);
  }
});
