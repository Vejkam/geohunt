import { useEffect, useRef } from "react";

declare global {
  interface Window {
    google: any;
  }
}

type LatLng = { lat: number; lng: number };

type RoundResultMapProps = {
  actual: LatLng;
  guess: LatLng;
  distanceKm: number;
  className?: string;
  style?: React.CSSProperties;
};

export default function RoundResultMap({
  actual,
  guess,
  distanceKm,
  className,
  style,
}: RoundResultMapProps) {
  const mapRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    let map: google.maps.Map | null = null;
    let markers: google.maps.Marker[] = [];
    let polyline: google.maps.Polyline | null = null;
    let retry: number | null = null;

    const init = () => {
      if (!mapRef.current) return;
      if (!window.google?.maps) {
        retry = window.setTimeout(init, 120);
        return;
      }

      const identical =
        Math.abs(actual.lat - guess.lat) < 0.00005 &&
        Math.abs(actual.lng - guess.lng) < 0.00005;

      map = new window.google.maps.Map(mapRef.current, {
        mapTypeControl: false,
        streetViewControl: false,
        fullscreenControl: false,
        clickableIcons: false,
        gestureHandling: "none",
        disableDoubleClickZoom: true,
      });

      const actualLatLng = new window.google.maps.LatLng(actual.lat, actual.lng);
      const guessLatLng = new window.google.maps.LatLng(guess.lat, guess.lng);
      const bounds = new window.google.maps.LatLngBounds();
      bounds.extend(actualLatLng);
      bounds.extend(guessLatLng);

      if (!map) return;

      if (identical) {
        map.setCenter(actualLatLng);
        map.setZoom(6);
      } else {
        map.fitBounds(bounds, { top: 24, right: 24, bottom: 24, left: 24 });
      }

      markers = [
        new window.google.maps.Marker({
          position: actualLatLng,
          map,
          title: "Actual location",
          icon: {
            path: window.google.maps.SymbolPath.CIRCLE,
            scale: 8,
            fillColor: "#22c55e",
            fillOpacity: 1,
            strokeColor: "#ffffff",
            strokeWeight: 2,
          },
        }),
        new window.google.maps.Marker({
          position: guessLatLng,
          map,
          title: "Your guess",
          icon: {
            path: window.google.maps.SymbolPath.CIRCLE,
            scale: 8,
            fillColor: "#f97316",
            fillOpacity: 1,
            strokeColor: "#ffffff",
            strokeWeight: 2,
          },
        }),
      ];

      if (!identical) {
        polyline = new window.google.maps.Polyline({
          map,
          path: [actualLatLng, guessLatLng],
          strokeOpacity: 0,
          icons: [
            {
              icon: {
                path: "M 0,-1 0,1",
                strokeOpacity: 0.8,
                strokeWeight: 2,
                strokeColor: "#334155",
              },
              offset: "0",
              repeat: "12px",
            },
          ],
          clickable: false,
        });
      }
    };

    init();

    return () => {
      if (retry !== null) window.clearTimeout(retry);
      markers.forEach((m) => m.setMap(null));
      if (polyline) polyline.setMap(null);
      if (map && window.google?.maps?.event?.clearInstanceListeners) {
        window.google.maps.event.clearInstanceListeners(map);
      }
    };
  }, [actual.lat, actual.lng, guess.lat, guess.lng, distanceKm]);

  return (
    <div className={className}>
      <div
        ref={mapRef}
        className="w-full h-56 rounded-lg overflow-hidden border border-slate-600 bg-slate-800"
        style={style}
        aria-label="Round map showing actual vs guess"
      />
      <div className="text-xs text-blue-200 mt-2">
        <span className="font-semibold text-green-400">Actual</span> ·{" "}
        <span className="font-semibold text-orange-400">Your guess</span> · Distance:{" "}
        {distanceKm.toFixed(1)} km
      </div>
    </div>
  );
}
