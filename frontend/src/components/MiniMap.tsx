/// <reference types="google.maps" />
import { useEffect, useRef, useState } from "react";

declare global {
  interface Window {
    google: any;
  }
}

type LatLngLiteral = google.maps.LatLngLiteral;

type MiniMapProps = {
  onSelect?: (coords: LatLngLiteral) => void;
  initialCenter?: LatLngLiteral;
  initialZoom?: number;
  className?: string;
  style?: React.CSSProperties;
  minZoomOnSelect?: number;
};

function waitForGoogleMaps(): Promise<void> {
  return new Promise((resolve, reject) => {
    if (window.google?.maps) {
      return resolve();
    }

    const startTime = Date.now();
    const checkInterval = setInterval(() => {
      if (window.google?.maps) {
        clearInterval(checkInterval);
        resolve();
      } else if (Date.now() - startTime > 10000) {
        clearInterval(checkInterval);
        reject(new Error('Google Maps failed to load within 10 seconds'));
      }
    }, 100);
  });
}

export default function MiniMap({
  onSelect,
  initialCenter = { lat: 0, lng: 0 },
  initialZoom = 1,
  className,
  style = { width: "100%", height: 240, position: "relative" },
  minZoomOnSelect = 3,
}: MiniMapProps) {
  const containerRef = useRef<HTMLDivElement | null>(null);
  const mapRef = useRef<google.maps.Map | null>(null);
  const markerRef = useRef<google.maps.Marker | null>(null);
  const clickListenerRef = useRef<google.maps.MapsEventListener | null>(null);
  const onSelectRef = useRef<typeof onSelect>(onSelect);

  const [loadError, setLoadError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    onSelectRef.current = onSelect;
  }, [onSelect]);

  useEffect(() => {
    if (mapRef.current && clickListenerRef.current && markerRef.current) {
      return;
    }

    if (mapRef.current) {
      if (!markerRef.current) {
        markerRef.current = new window.google.maps.Marker({
          map: mapRef.current,
          visible: false,
          draggable: false,
        });
      }

      if (!clickListenerRef.current) {
        clickListenerRef.current = mapRef.current.addListener(
          "click",
          (event: google.maps.MapMouseEvent) => {
            const lat = event.latLng?.lat();
            const lng = event.latLng?.lng();

            if (lat == null || lng == null) {
              console.warn('[MiniMap] Invalid lat/lng from click event');
              return;
            }

            const position: LatLngLiteral = { lat, lng };

            markerRef.current!.setPosition(position);
            markerRef.current!.setVisible(true);

            onSelectRef.current?.(position);
          }
        );
      }
      return;
    }

    let cancelled = false;

    (async () => {
      try {
        await waitForGoogleMaps();

        if (cancelled) {
          return;
        }

        if (!containerRef.current) {
          throw new Error('Container ref not available');
        }

        mapRef.current = new window.google.maps.Map(containerRef.current, {
          center: initialCenter,
          zoom: initialZoom,
          fullscreenControl: false,
          mapTypeControl: false,
          streetViewControl: false,
          disableDefaultUI: false,
          clickableIcons: true,
          gestureHandling: 'greedy',
        });

        await new Promise(resolve => setTimeout(resolve, 100));

        markerRef.current = new window.google.maps.Marker({
          map: mapRef.current,
          visible: false,
          draggable: false,
        });

        if (!mapRef.current) return;

        clickListenerRef.current = mapRef.current.addListener(
          "click",
          (event: google.maps.MapMouseEvent) => {
            const lat = event.latLng?.lat();
            const lng = event.latLng?.lng();

            if (lat == null || lng == null) {
              console.warn('[MiniMap] Invalid lat/lng from click event');
              return;
            }

            const position: LatLngLiteral = { lat, lng };

            markerRef.current!.setPosition(position);
            markerRef.current!.setVisible(true);

            onSelectRef.current?.(position);
          }
        );

        setIsLoading(false);
      } catch (err) {
        console.error('[MiniMap] Failed to initialize:', err);
        setLoadError(err instanceof Error ? err.message : 'Failed to load map');
        setIsLoading(false);
      }
    })();

    return () => {
      cancelled = true;
      if (clickListenerRef.current) {
        clickListenerRef.current.remove();
        clickListenerRef.current = null;
      }
      if (markerRef.current) {
        markerRef.current.setMap(null);
        markerRef.current = null;
      }
    };
  }, [initialCenter, initialZoom, minZoomOnSelect]);

  if (loadError) {
    return (
      <div className={className} style={style}>
        <div style={{ 
          position: "absolute", 
          inset: 0, 
          display: "flex", 
          alignItems: "center", 
          justifyContent: "center",
          background: "#fee",
          color: "#c00",
          fontSize: 14,
          padding: 16,
          textAlign: "center",
          flexDirection: "column",
          gap: 8
        }}>
          <strong>MiniMap Error</strong>
          <div>{loadError}</div>
        </div>
      </div>
    );
  }

  return (
    <div className={className} style={{ ...style, position: 'relative', overflow: 'hidden' }}>
      {isLoading && (
        <div style={{
          position: "absolute",
          inset: 0,
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          background: "#f0f0f0",
          fontSize: 14,
          color: "#666",
          zIndex: 10
        }}>
          Loading map...
        </div>
      )}
      <div
        ref={containerRef}
        style={{ position: "absolute", inset: 0 }}
      />
    </div>
  );
}