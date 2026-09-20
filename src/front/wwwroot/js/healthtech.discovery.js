window.healthTechDiscovery = {
    getCurrentPosition: () => new Promise((resolve, reject) => {
        if (!navigator.geolocation) {
            reject(new Error("geolocation_unavailable"));
            return;
        }

        navigator.geolocation.getCurrentPosition(
            position => resolve({
                latitude: position.coords.latitude,
                longitude: position.coords.longitude
            }),
            error => reject(error),
            {
                enableHighAccuracy: false,
                timeout: 8000,
                maximumAge: 300000
            });
    })
};
